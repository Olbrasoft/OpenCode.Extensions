using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKeyboardProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First update Id=1 (Keyboard -> HumanInput) - no conflict
            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Human input (keyboard, voice, or combination)", "HumanInput" });

            // Update Id=2 description only - no conflict
            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "System pipeline - voice processing orchestration");

            // Delete old records 3-9 to avoid unique constraint violations
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 4);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 5);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 6);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 7);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 8);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 9);

            // Insert new records with correct values
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 3, "Claude models via Anthropic API", "Anthropic" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 4, "GPT models via OpenAI API", "OpenAI" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 5, "GitHub Copilot (routes to various models)", "GitHubCopilot" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 6, "Gemini models via Google AI", "Google" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 7, "Azure OpenAI Service", "AzureOpenAI" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 8, "Grok models via xAI", "xAI" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Direct typing to terminal", "Keyboard" });

            migrationBuilder.UpdateData(
                table: "Providers",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "ContinuousListener - voice input");

            // Delete new records
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 4);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 5);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 6);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 7);
            migrationBuilder.DeleteData(table: "Providers", keyColumn: "Id", keyValue: 8);

            // Re-insert old records
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 3, "Combination of keyboard and voice", "HumanCombination" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 4, "Claude models via Anthropic API", "Anthropic" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 5, "GPT models via OpenAI API", "OpenAI" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 6, "GitHub Copilot (routes to various models)", "GitHubCopilot" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 7, "Gemini models via Google AI", "Google" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 8, "Azure OpenAI Service", "AzureOpenAI" });
            
            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 9, "Grok models via xAI", "xAI" });
        }
    }
}
