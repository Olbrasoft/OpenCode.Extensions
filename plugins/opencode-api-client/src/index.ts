/**
 * OpenCode API Client Plugin v3 - Monolog Support
 * 
 * Plugin that connects OpenCode to our C# API for session and monolog logging.
 * 
 * Monolog = continuous speech by one participant until the other speaks.
 * - User messages without assistant response are joined into one monolog
 * - Assistant streaming responses form one monolog
 * - Monolog is closed when the other participant speaks
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

// Constants
const ROLE_USER = 1;
const ROLE_ASSISTANT = 2;
const DEFAULT_PARTICIPANT_ID = "00000000-0000-0000-0000-000000000001"; // Default user
const DEFAULT_PROVIDER_ID = 1; // OpenCode
const DEFAULT_MODE_ID = 1; // Build

// State tracking
interface MonologState {
  monologId: number | null;
  role: number;
  content: string;
  firstMessageId: string;
}

// Current monolog states by session
const sessionStates = new Map<string, {
  userMonolog: MonologState | null;
  assistantMonolog: MonologState | null;
  dbSessionId: number | null;
}>();

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

/**
 * Get or initialize session state
 */
function getSessionState(sessionId: string) {
  if (!sessionStates.has(sessionId)) {
    sessionStates.set(sessionId, {
      userMonolog: null,
      assistantMonolog: null,
      dbSessionId: null
    });
  }
  return sessionStates.get(sessionId)!;
}

/**
 * API: Upsert session
 */
async function apiUpsertSession(
  sessionId: string,
  title: string | null,
  directory: string | null,
  createdAt: number
): Promise<number | null> {
  try {
    const payload = {
      sessionId,
      title,
      workingDirectory: directory,
      createdAt: epochToIso(createdAt)
    };

    log(`API: POST /api/sessions - ${sessionId}`);

    const response = await fetch(`${API_BASE_URL}/api/sessions`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      const result = await response.json() as { id: number; sessionId: string };
      log(`API: Session upserted, DB ID: ${result.id}`);
      return result.id;
    } else {
      const errorText = await response.text();
      log(`API ERROR: ${response.status} - ${errorText}`);
      return null;
    }
  } catch (error) {
    log(`API EXCEPTION: ${error}`);
    return null;
  }
}

/**
 * API: Get open monolog
 */
async function apiGetOpenMonolog(dbSessionId: number, role: number): Promise<{ id: number; content: string } | null> {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/monologs/open?sessionId=${dbSessionId}&role=${role}`
    );

    if (response.ok) {
      const result = await response.json() as { id: number; content: string };
      return result;
    }
    return null;
  } catch (error) {
    log(`API EXCEPTION (getOpenMonolog): ${error}`);
    return null;
  }
}

/**
 * API: Create monolog
 */
async function apiCreateMonolog(
  dbSessionId: number,
  parentMonologId: number | null,
  role: number,
  firstMessageId: string,
  content: string,
  startedAt: number
): Promise<number | null> {
  try {
    const payload = {
      sessionId: dbSessionId,
      parentMonologId,
      role,
      firstMessageId,
      content,
      participantId: DEFAULT_PARTICIPANT_ID,
      providerId: DEFAULT_PROVIDER_ID,
      modeId: DEFAULT_MODE_ID,
      startedAt: epochToIso(startedAt)
    };

    log(`API: POST /api/monologs - role: ${role}, content: "${content.substring(0, 50)}..."`);

    const response = await fetch(`${API_BASE_URL}/api/monologs`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      const result = await response.json() as { id: number };
      log(`API: Monolog created, ID: ${result.id}`);
      return result.id;
    } else {
      const errorText = await response.text();
      log(`API ERROR (createMonolog): ${response.status} - ${errorText}`);
      return null;
    }
  } catch (error) {
    log(`API EXCEPTION (createMonolog): ${error}`);
    return null;
  }
}

/**
 * API: Append content to monolog
 */
async function apiAppendContent(monologId: number, content: string): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/monologs/${monologId}/append`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ content })
    });

    return response.ok;
  } catch (error) {
    log(`API EXCEPTION (appendContent): ${error}`);
    return false;
  }
}

/**
 * API: Update content of monolog
 */
async function apiUpdateContent(monologId: number, content: string): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/monologs/${monologId}/content`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ content })
    });

    return response.ok;
  } catch (error) {
    log(`API EXCEPTION (updateContent): ${error}`);
    return false;
  }
}

/**
 * API: Close monolog
 */
async function apiCloseMonolog(
  monologId: number,
  lastMessageId: string,
  finalContent: string | null,
  completedAt: number,
  isAborted: boolean,
  tokensInput: number | null = null,
  tokensOutput: number | null = null,
  cost: number | null = null
): Promise<boolean> {
  try {
    const payload = {
      lastMessageId,
      finalContent,
      completedAt: epochToIso(completedAt),
      isAborted,
      tokensInput,
      tokensOutput,
      cost
    };

    log(`API: PUT /api/monologs/${monologId}/close - aborted: ${isAborted}`);

    const response = await fetch(`${API_BASE_URL}/api/monologs/${monologId}/close`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      log(`API: Monolog ${monologId} closed`);
      return true;
    } else {
      const errorText = await response.text();
      log(`API ERROR (closeMonolog): ${response.status} - ${errorText}`);
      return false;
    }
  } catch (error) {
    log(`API EXCEPTION (closeMonolog): ${error}`);
    return false;
  }
}

// Track processed sessions to avoid duplicate API calls
const processedSessions = new Set<string>();

/**
 * Handle session.updated event
 */
async function handleSessionUpdated(properties: Record<string, unknown>): Promise<void> {
  const info = properties?.info as Record<string, unknown> | undefined;
  if (!info?.id) return;

  const sessionId = info.id as string;
  const title = (info.title as string) || null;
  const directory = (info.directory as string) || null;
  const createdAt = (info.time as { created?: number })?.created || Date.now();

  // Avoid duplicate calls
  const cacheKey = `${sessionId}:${title}`;
  if (processedSessions.has(cacheKey)) return;
  processedSessions.add(cacheKey);
  setTimeout(() => processedSessions.delete(cacheKey), 5000);

  log(`SESSION.UPDATED: ${sessionId} - "${title}"`);
  
  const dbSessionId = await apiUpsertSession(sessionId, title, directory, createdAt);
  if (dbSessionId) {
    const state = getSessionState(sessionId);
    state.dbSessionId = dbSessionId;
  }
}

/**
 * Handle message.updated event
 * This is called when a message is created or updated (final state)
 */
async function handleMessageUpdated(properties: Record<string, unknown>, sessionId: string): Promise<void> {
  const info = properties?.info as Record<string, unknown> | undefined;
  if (!info?.id) return;

  const messageId = info.id as string;
  const role = info.role as string;
  const text = ((info.parts as Array<{ type: string; text: string }>) || [])
    .filter(p => p.type === "text")
    .map(p => p.text)
    .join("\n");

  const state = getSessionState(sessionId);
  if (!state.dbSessionId) {
    log(`WARN: No DB session ID for ${sessionId}`);
    return;
  }

  const now = Date.now();

  if (role === "user") {
    // User message - check if we need to close assistant monolog first
    if (state.assistantMonolog?.monologId) {
      await apiCloseMonolog(
        state.assistantMonolog.monologId,
        state.assistantMonolog.firstMessageId,
        state.assistantMonolog.content,
        now,
        false
      );
      state.assistantMonolog = null;
    }

    // Check for existing open user monolog
    if (state.userMonolog?.monologId) {
      // Append to existing user monolog (no-reply scenario)
      await apiAppendContent(state.userMonolog.monologId, text);
      state.userMonolog.content += "\n\n" + text;
      log(`MONOLOG: Appended to user monolog ${state.userMonolog.monologId}`);
    } else {
      // Create new user monolog
      const parentId = state.assistantMonolog?.monologId || null;
      const monologId = await apiCreateMonolog(
        state.dbSessionId,
        parentId,
        ROLE_USER,
        messageId,
        text,
        now
      );
      if (monologId) {
        state.userMonolog = {
          monologId,
          role: ROLE_USER,
          content: text,
          firstMessageId: messageId
        };
        log(`MONOLOG: Created user monolog ${monologId}`);
      }
    }
  } else if (role === "assistant") {
    // Assistant message - close user monolog first
    if (state.userMonolog?.monologId) {
      await apiCloseMonolog(
        state.userMonolog.monologId,
        state.userMonolog.firstMessageId,
        state.userMonolog.content,
        now,
        false
      );
      const closedUserMonologId = state.userMonolog.monologId;
      state.userMonolog = null;

      // Create assistant monolog with user as parent
      const monologId = await apiCreateMonolog(
        state.dbSessionId,
        closedUserMonologId,
        ROLE_ASSISTANT,
        messageId,
        text,
        now
      );
      if (monologId) {
        state.assistantMonolog = {
          monologId,
          role: ROLE_ASSISTANT,
          content: text,
          firstMessageId: messageId
        };
        log(`MONOLOG: Created assistant monolog ${monologId}`);
      }
    } else if (state.assistantMonolog?.monologId) {
      // Update existing assistant monolog (streaming complete)
      await apiUpdateContent(state.assistantMonolog.monologId, text);
      state.assistantMonolog.content = text;
      log(`MONOLOG: Updated assistant monolog ${state.assistantMonolog.monologId}`);
    }
  }
}

/**
 * Handle message.part.updated event (streaming)
 */
async function handleMessagePartUpdated(properties: Record<string, unknown>, sessionId: string): Promise<void> {
  const part = properties?.part as Record<string, unknown> | undefined;
  if (part?.type !== "text") return;

  const text = (part.text as string) || "";
  if (text.length === 0) return;

  const state = getSessionState(sessionId);
  
  // Update assistant content during streaming
  if (state.assistantMonolog?.monologId) {
    // Throttle updates - only update every 500ms worth of text or 100 chars
    const contentDiff = text.length - state.assistantMonolog.content.length;
    if (contentDiff > 100) {
      await apiUpdateContent(state.assistantMonolog.monologId, text);
      state.assistantMonolog.content = text;
    }
  }
}

/**
 * Handle session becoming idle (all monologs should be closed)
 */
async function handleSessionIdle(sessionId: string): Promise<void> {
  const state = getSessionState(sessionId);
  const now = Date.now();

  if (state.userMonolog?.monologId) {
    await apiCloseMonolog(
      state.userMonolog.monologId,
      state.userMonolog.firstMessageId,
      state.userMonolog.content,
      now,
      false
    );
    state.userMonolog = null;
  }

  if (state.assistantMonolog?.monologId) {
    await apiCloseMonolog(
      state.assistantMonolog.monologId,
      state.assistantMonolog.firstMessageId,
      state.assistantMonolog.content,
      now,
      false
    );
    state.assistantMonolog = null;
  }
}

log("================================================================================");
log("==================== OPENCODE API CLIENT PLUGIN v3 (MONOLOGS) =================");
log(`==================== API: ${API_BASE_URL} ======================================`);
log("================================================================================");

/**
 * OpenCode API Client Plugin v3
 */
export const OpenCodeApiClientPlugin: Plugin = async (context) => {
  log("==================== PLUGIN INITIALIZED ====================");
  log(`Directory: ${context.directory}`);

  return {
    // Generic event handler
    event: async ({ event }) => {
      const eventType = event?.type as string;
      const properties = event?.properties as Record<string, unknown>;
      const sessionId = properties?.sessionID as string || "";

      switch (eventType) {
        case "session.updated":
        case "session.created":
          await handleSessionUpdated(properties);
          break;
        
        case "session.status":
          const status = (properties?.status as { type?: string })?.type;
          if (status === "idle" && sessionId) {
            log(`SESSION.IDLE: ${sessionId}`);
            await handleSessionIdle(sessionId);
          }
          break;

        case "message.updated":
          if (sessionId) {
            await handleMessageUpdated(properties, sessionId);
          }
          break;

        case "message.part.updated":
          if (sessionId) {
            await handleMessagePartUpdated(properties, sessionId);
          }
          break;

        // Ignore noisy events
        case "session.diff":
        case "tool.execute.before":
        case "tool.execute.after":
          break;

        default:
          if (eventType && !eventType.startsWith("message.part")) {
            log(`EVENT: ${eventType}`);
          }
          break;
      }
    },
  };
};

export default OpenCodeApiClientPlugin;
