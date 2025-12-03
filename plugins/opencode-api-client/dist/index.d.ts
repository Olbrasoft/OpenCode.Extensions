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
/**
 * OpenCode API Client Plugin
 *
 * Uses generic event handler to catch all events and route them appropriately.
 * Uses session.idle event + client API to get full message content.
 */
export declare const OpenCodeApiClientPlugin: Plugin;
export default OpenCodeApiClientPlugin;
//# sourceMappingURL=index.d.ts.map