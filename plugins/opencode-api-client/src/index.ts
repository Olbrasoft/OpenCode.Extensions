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

// Track processed sessions to avoid duplicate API calls
const processedSessions = new Set<string>();

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
 * OpenCode API Client Plugin
 * 
 * Uses generic event handler to catch all events and route them appropriately.
 */
export const OpenCodeApiClientPlugin: Plugin = async (context) => {
  log("==================== PLUGIN INITIALIZED ====================");
  log(`Directory: ${context.directory}`);
  log(`Worktree: ${context.worktree}`);
  log(`Project ID: ${context.project?.id}`);

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
        
        case "session.status":
          // Just log status changes
          const status = (properties?.status as { type?: string })?.type;
          if (status === "idle") {
            log(`SESSION.IDLE: ${properties?.sessionID}`);
          }
          break;

        case "message.updated":
          // Log message updates (for future implementation)
          const msgInfo = properties?.info as Record<string, unknown> | undefined;
          if (msgInfo?.id) {
            log(`MESSAGE.UPDATED: ${msgInfo.id} (role: ${msgInfo.role})`);
          }
          break;

        case "message.part.updated":
          // Log text message parts
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
