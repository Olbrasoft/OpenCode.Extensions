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
/**
 * OpenCode API Client Plugin v3
 */
export declare const OpenCodeApiClientPlugin: Plugin;
export default OpenCodeApiClientPlugin;
//# sourceMappingURL=index.d.ts.map