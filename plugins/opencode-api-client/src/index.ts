/**
 * OpenCode API Client Plugin
 * 
 * Plugin that connects OpenCode to our custom C# API for session and message logging.
 * 
 * API Endpoint: http://localhost:5100
 * 
 * IMPORTANT: This plugin MUST NOT use console.log() as it breaks OpenCode's TUI.
 * All logging goes to /tmp/opencode-plugin-logs/api-client.log
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
  agent: string;
  model: { providerID: string; modelID: string };
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

type MessageInfo = UserMessageInfo | AssistantMessageInfo;

function isUserMessage(info: MessageInfo): info is UserMessageInfo {
  return info.role === "user";
}

function isAssistantMessage(info: MessageInfo): info is AssistantMessageInfo {
  return info.role === "assistant";
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

/**
 * Convert mode string to numeric value
 */
function modeToNumber(mode: string): number | null {
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

/**
 * Handle message.updated event
 */
async function handleMessageUpdated(properties: Record<string, unknown>): Promise<void> {
  const info = properties?.info as MessageInfo | undefined;
  if (!info?.id) {
    return;
  }

  // Only process completed assistant messages or user messages
  // Assistant messages should have completed time, user messages are always complete
  if (isAssistantMessage(info) && !info.time.completed) {
    // Assistant message still in progress - wait for completion
    return;
  }

  // Avoid duplicate API calls
  const cacheKey = `${info.id}:${info.time.created}`;
  if (processedMessages.has(cacheKey)) {
    return;
  }
  processedMessages.add(cacheKey);
  
  // Clean up old entries after 30 seconds
  setTimeout(() => processedMessages.delete(cacheKey), 30000);

  log(`MESSAGE.UPDATED: ${info.id} (role: ${info.role})`);

  // Build the request based on message type
  let request: CreateMessageRequest;

  if (isUserMessage(info)) {
    request = {
      messageId: info.id,
      sessionId: info.sessionID,
      role: 0, // User
      mode: null, // User messages don't have mode
      participantIdentifier: "user-jirka", // Will be resolved by API
      providerName: "HumanInput",
      content: null, // Content comes from parts, we'll need to collect it separately
      tokensInput: null,
      tokensOutput: null,
      cost: null,
      createdAt: epochToIso(info.time.created),
      parentMessageId: null
    };
  } else {
    // Assistant message
    request = {
      messageId: info.id,
      sessionId: info.sessionID,
      role: 1, // Assistant
      mode: modeToNumber(info.mode),
      participantIdentifier: info.modelID, // e.g., "claude-sonnet-4-20250514"
      providerName: info.providerID,       // e.g., "anthropic"
      content: null, // Content comes from parts
      tokensInput: info.tokens.input + info.tokens.cache.read,
      tokensOutput: info.tokens.output,
      cost: info.cost,
      createdAt: epochToIso(info.time.created),
      parentMessageId: info.parentID
    };
  }

  await createMessage(request);
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
 * Uses generic event handler to catch all events and route them appropriately.
 * Uses session.idle event + client API to get full message content.
 */
export const OpenCodeApiClientPlugin: Plugin = async (context) => {
  log("==================== PLUGIN INITIALIZED ====================");
  log(`Directory: ${context.directory}`);
  log(`Worktree: ${context.worktree}`);
  log(`Project ID: ${context.project?.id}`);

  // Get client for API calls
  const client = context.client;
  
  // Track last processed message to avoid duplicates
  let lastProcessedMessageId: string | null = null;
  let isProcessingIdle = false;

  /**
   * Handle session.idle - fetch full messages with content and send to API
   */
  async function handleSessionIdle(sessionId: string): Promise<void> {
    if (isProcessingIdle) {
      log("SESSION.IDLE: skipping - already processing");
      return;
    }
    isProcessingIdle = true;

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

      // Find the last assistant message with content that we haven't processed
      const lastAssistantMessage = [...messages]
        .reverse()
        .find((m) => {
          if (m.info.role !== "assistant") return false;
          const parts = m.parts as Array<{ type: string; text?: string; content?: string }>;
          return parts?.some(p => 
            (p.type === "text" || p.type === "markdown") && (p.text || p.content)
          );
        });

      if (!lastAssistantMessage) {
        log("SESSION.IDLE: no assistant message with content found");
        return;
      }

      const msgInfo = lastAssistantMessage.info as AssistantMessageInfo;
      
      if (msgInfo.id === lastProcessedMessageId) {
        log(`SESSION.IDLE: message ${msgInfo.id} already processed`);
        return;
      }

      lastProcessedMessageId = msgInfo.id;

      // Extract text content from parts
      const parts = lastAssistantMessage.parts as Array<{ type: string; text?: string; content?: string }>;
      const textContent = extractTextFromParts(parts);
      
      log(`SESSION.IDLE: processing message ${msgInfo.id} with ${textContent.length} chars of content`);

      // Update the message in database with content
      // We use a PATCH/PUT endpoint or re-send with content
      const request: CreateMessageRequest = {
        messageId: msgInfo.id,
        sessionId: msgInfo.sessionID,
        role: 1, // Assistant
        mode: modeToNumber(msgInfo.mode),
        participantIdentifier: msgInfo.modelID,
        providerName: msgInfo.providerID,
        content: textContent || null,
        tokensInput: msgInfo.tokens.input + msgInfo.tokens.cache.read,
        tokensOutput: msgInfo.tokens.output,
        cost: msgInfo.cost,
        createdAt: epochToIso(msgInfo.time.created),
        parentMessageId: msgInfo.parentID
      };

      await createMessage(request);
      log(`SESSION.IDLE: message ${msgInfo.id} sent with content`);

    } catch (error) {
      log(`SESSION.IDLE ERROR: ${error}`);
    } finally {
      isProcessingIdle = false;
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
          // This is the main event for processing completed messages with content
          const sessionId = properties?.sessionID as string;
          if (sessionId) {
            await handleSessionIdle(sessionId);
          }
          break;

        case "message.updated":
          // Still process for metadata (tokens, cost) but without content
          await handleMessageUpdated(properties);
          break;

        case "message.part.updated":
          // Log text message parts for debugging
          const part = properties?.part as Record<string, unknown> | undefined;
          if (part?.type === "text") {
            const text = (part.text as string) || "";
            if (text.length > 0) {
              const preview = text.length > 50 ? text.substring(0, 50) + "..." : text;
              log(`MESSAGE.TEXT: "${preview}"`);
            }
          }
          break;

        // Ignore noisy events
        case "session.diff":
        case "tool.execute.before":
        case "tool.execute.after":
          break;

        default:
          // Log unknown events for debugging
          if (eventType && !eventType.startsWith("message.part")) {
            log(`EVENT: ${eventType}`);
          }
          break;
      }
    },

    // Keep specific handlers for tool events (these work differently)
    "tool.execute.before": async ({ tool, sessionID }) => {
      log(`TOOL.BEFORE: ${tool}`);
    },

    "tool.execute.after": async ({ tool, sessionID }) => {
      log(`TOOL.AFTER: ${tool}`);
    },
  };
};

// Default export
export default OpenCodeApiClientPlugin;
