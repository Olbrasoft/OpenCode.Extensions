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
/**
 * OpenCode API Client Plugin
 *
 * Messages are created ONLY on session.idle event when we have full content.
 * No message.updated handler - that would create messages without content.
 */
export declare const OpenCodeApiClientPlugin: Plugin;
export default OpenCodeApiClientPlugin;
//# sourceMappingURL=index.d.ts.map