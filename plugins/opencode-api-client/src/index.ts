/**
 * OpenCode API Client Plugin
 * 
 * Plugin that connects OpenCode to our custom C# API for session and message logging.
 * 
 * API Endpoint: http://localhost:5100
 * 
 * IMPORTANT: This plugin MUST NOT use console.log() as it breaks OpenCode's TUI.
 * All logging goes to /tmp/opencode-plugin-logs/api-client.log
 * 
 * Messages are ONLY created when session.idle fires - this ensures we have full content.
 */

import type { Plugin } from "@opencode-ai/plugin";
import { appendFileSync, mkdirSync, existsSync } from "fs";

// Configuration
const API_BASE_URL = "http://localhost:5100";
const LOG_DIR = "/tmp/opencode-plugin-logs";
const LOG_FILE = `${LOG_DIR}/api-client.log`;

// Ensure log directory exists
if (!existsSync(LOG_DIR)) {
  try {
    mkdirSync(LOG_DIR, { recursive: true });
  } catch {
    // Ignore
  }
}

/**
 * Log a message to file (NEVER to console!)
 */
function log(message: string): void {
  const timestamp = new Date().toISOString();
  try {
    appendFileSync(LOG_FILE, `[${timestamp}] ${message}\n`);
  } catch {
    // Silently ignore
  }
}

/**
 * Convert OpenCode timestamp (epoch ms) to ISO string
 */
function epochToIso(epochMs: number): string {
  return new Date(epochMs).toISOString();
}

// ============================================================================
// API Request Types
// ============================================================================

interface CreateMessageRequest {
  messageId: string;
  sessionId: string;
  role: number;           // 0 = User, 1 = Assistant
  mode: number | null;    // 1 = Build, 2 = Plan
  participantIdentifier: string;
  providerName: string;
  content: string | null;
  tokensInput: number | null;
  tokensOutput: number | null;
  cost: number | null;
  createdAt: string;
  parentMessageId: string | null;
}

// ============================================================================
// Message Type Guards
// ============================================================================

interface UserMessageInfo {
  id: string;
  sessionID: string;
  role: "user";
  time: { created: number };
}

interface AssistantMessageInfo {
  id: string;
  sessionID: string;
  role: "assistant";
  time: { created: number; completed?: number };
  parentID: string;
  modelID: string;
  providerID: string;
  mode: string;
  cost: number;
  tokens: {
    input: number;
    output: number;
    reasoning: number;
    cache: { read: number; write: number };
  };
}

/**
 * Send session to our C# API (upsert - creates or updates)
 */
async function upsertSession(
  sessionId: string,
  title: string | null,
  directory: string | null,
  createdAt: number
): Promise<void> {
  try {
    const payload = {
      sessionId,
      title,
      workingDirectory: directory,
      createdAt: epochToIso(createdAt)
    };

    log(`API CALL: POST /api/sessions - ${sessionId}`);

    const response = await fetch(`${API_BASE_URL}/api/sessions`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      const result = await response.json() as { id: number; sessionId: string };
      log(`API SUCCESS: Session upserted with internal ID ${result.id}`);
    } else {
      const errorText = await response.text();
      log(`API ERROR: ${response.status} ${response.statusText} - ${errorText}`);
    }
  } catch (error) {
    log(`API EXCEPTION: ${error}`);
  }
}

// Track processed sessions and messages to avoid duplicate API calls
const processedSessions = new Set<string>();
const processedMessages = new Set<string>();

// Track which sessions are currently being processed (global lock per session)
const processingSessionsLock = new Set<string>();

/**
 * Convert mode string to numeric value
 */
function modeToNumber(mode: string | undefined): number | null {
  if (!mode) return null;
  switch (mode.toLowerCase()) {
    case "build": return 1;
    case "plan": return 2;
    default: return null;
  }
}

/**
 * Send message to our C# API
 */
async function createMessage(request: CreateMessageRequest): Promise<void> {
  try {
    log(`API CALL: POST /api/messages - ${request.messageId} (${request.role === 0 ? "user" : "assistant"})`);

    const response = await fetch(`${API_BASE_URL}/api/messages`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });

    if (response.ok) {
      const result = await response.json() as { id: number; messageId: string };
      log(`API SUCCESS: Message created with internal ID ${result.id}`);
    } else {
      const errorText = await response.text();
      log(`API ERROR: ${response.status} ${response.statusText} - ${errorText}`);
    }
  } catch (error) {
    log(`API EXCEPTION: ${error}`);
  }
}

/**
 * Handle session.updated event
 */
async function handleSessionUpdated(properties: Record<string, unknown>): Promise<void> {
  const info = properties?.info as Record<string, unknown> | undefined;
  if (!info?.id) {
    return;
  }

  const sessionId = info.id as string;
  const title = (info.title as string) || null;
  const directory = (info.directory as string) || null;
  const createdAt = (info.time as { created?: number })?.created || Date.now();

  // Avoid duplicate calls for the same session in quick succession
  const cacheKey = `${sessionId}:${title}`;
  if (processedSessions.has(cacheKey)) {
    return;
  }
  processedSessions.add(cacheKey);
  
  // Clean up old entries after 5 seconds
  setTimeout(() => processedSessions.delete(cacheKey), 5000);

  log(`SESSION.UPDATED: ${sessionId} - "${title}"`);
  await upsertSession(sessionId, title, directory, createdAt);
}

log("================================================================================");
log("==================== OPENCODE API CLIENT PLUGIN LOADED ========================");
log(`==================== API: ${API_BASE_URL} ======================================`);
log("================================================================================");

/**
 * Extract text content from message parts
 * Handles multiple content types: text, markdown, content
 */
function extractTextFromParts(parts: Array<{ type: string; text?: string; content?: string }>): string {
  return parts
    .filter((part) => (part.type === "text" || part.type === "markdown") && (part.text || part.content))
    .map((part) => part.text || part.content || "")
    .join("\n");
}

/**
 * OpenCode API Client Plugin
 * 
 * Messages are created ONLY on session.idle event when we have full content.
 * No message.updated handler - that would create messages without content.
 */
export const OpenCodeApiClientPlugin: Plugin = async (context) => {
  log("==================== PLUGIN INITIALIZED ====================");
  log(`Directory: ${context.directory}`);
  log(`Worktree: ${context.worktree}`);
  log(`Project ID: ${context.project?.id}`);

  // Get client for API calls
  const client = context.client;

  /**
   * Handle session.idle - fetch ALL messages with content and send to API
   */
  async function handleSessionIdle(sessionId: string): Promise<void> {
    // Use global lock per session to prevent duplicate processing
    if (processingSessionsLock.has(sessionId)) {
      log(`SESSION.IDLE: skipping ${sessionId} - already processing (global lock)`);
      return;
    }
    processingSessionsLock.add(sessionId);

    try {
      log(`SESSION.IDLE: Processing ${sessionId}`);
      
      if (!client) {
        log("SESSION.IDLE: client is null");
        return;
      }

      // Fetch messages from OpenCode API
      const messagesResponse = await client.session.messages({
        path: { id: sessionId },
      });

      const messages = messagesResponse.data ?? [];
      log(`SESSION.IDLE: fetched ${messages.length} messages`);

      // Sort messages by creation time to establish correct parent chain
      const sortedMessages = [...messages].sort((a, b) => {
        const timeA = (a.info as { time: { created: number } }).time.created;
        const timeB = (b.info as { time: { created: number } }).time.created;
        return timeA - timeB;
      });

      // Track the last processed message ID to build the chain
      // User message -> parent is previous assistant (or null for first)
      // Assistant message -> parent is the user message it responds to (from parentID)
      let lastAssistantMessageId: string | null = null;

      // Process all messages that haven't been sent yet
      for (const message of sortedMessages) {
        const msgInfo = message.info;
        
        // Skip if already processed
        if (processedMessages.has(msgInfo.id)) {
          continue;
        }

        // Extract text content from parts
        const parts = message.parts as Array<{ type: string; text?: string; content?: string }>;
        const textContent = extractTextFromParts(parts);
        
        // Skip messages without content
        if (!textContent || textContent.trim().length === 0) {
          log(`SESSION.IDLE: skipping message ${msgInfo.id} - no content`);
          continue;
        }

        // Mark as processed
        processedMessages.add(msgInfo.id);

        let request: CreateMessageRequest;

        if (msgInfo.role === "user") {
          const userInfo = msgInfo as UserMessageInfo;
          // User message: parent is the last assistant message (or null for first message in session)
          request = {
            messageId: userInfo.id,
            sessionId: userInfo.sessionID,
            role: 0, // User
            mode: null,
            participantIdentifier: "user-jirka",
            providerName: "HumanInput",
            content: textContent,
            tokensInput: null,
            tokensOutput: null,
            cost: null,
            createdAt: epochToIso(userInfo.time.created),
            parentMessageId: lastAssistantMessageId  // null for first, or previous assistant's ID
          };
        } else {
          const assistantInfo = msgInfo as AssistantMessageInfo;
          // Assistant message: parent is the user message it responds to (from OpenCode's parentID)
          request = {
            messageId: assistantInfo.id,
            sessionId: assistantInfo.sessionID,
            role: 1, // Assistant
            mode: modeToNumber(assistantInfo.mode),
            participantIdentifier: assistantInfo.modelID,
            providerName: assistantInfo.providerID,
            content: textContent,
            tokensInput: assistantInfo.tokens.input + assistantInfo.tokens.cache.read,
            tokensOutput: assistantInfo.tokens.output,
            cost: assistantInfo.cost,
            createdAt: epochToIso(assistantInfo.time.created),
            parentMessageId: assistantInfo.parentID
          };
          // Track this assistant message for the next user message's parent
          lastAssistantMessageId = assistantInfo.id;
        }

        log(`SESSION.IDLE: creating message ${msgInfo.id} (${msgInfo.role}) with ${textContent.length} chars, parent: ${request.parentMessageId ?? 'null'}`);
        await createMessage(request);
      }

      log(`SESSION.IDLE: done processing ${sessionId}`);

    } catch (error) {
      log(`SESSION.IDLE ERROR: ${error}`);
    } finally {
      processingSessionsLock.delete(sessionId);
    }
  }

  return {
    // Generic event handler - catches ALL events
    event: async ({ event }) => {
      const eventType = event?.type as string;
      const properties = event?.properties as Record<string, unknown>;

      // Route events to appropriate handlers
      switch (eventType) {
        case "session.updated":
        case "session.created":
          await handleSessionUpdated(properties);
          break;
        
        case "session.idle":
          // This is THE event for creating messages - with full content
          const sessionId = properties?.sessionID as string;
          if (sessionId) {
            await handleSessionIdle(sessionId);
          }
          break;

        // Ignore all other events - we don't need them
        default:
          break;
      }
    },
  };
};

// Default export
export default OpenCodeApiClientPlugin;
