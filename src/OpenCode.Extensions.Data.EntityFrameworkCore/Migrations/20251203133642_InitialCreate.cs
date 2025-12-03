using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkingDirectory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParticipantTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_ParticipantTypes_ParticipantTypeId",
                        column: x => x.ParticipantTypeId,
                        principalTable: "ParticipantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    ParentMessageId = table.Column<int>(type: "integer", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ModeId = table.Column<int>(type: "integer", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    TokensInput = table.Column<int>(type: "integer", nullable: true),
                    TokensOutput = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Modes_ModeId",
                        column: x => x.ModeId,
                        principalTable: "Modes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Build" },
                    { 2, "Plan" }
                });

            migrationBuilder.InsertData(
                table: "ParticipantTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Human" },
                    { 2, "AiModel" },
                    { 3, "Script" },
                    { 4, "System" }
                });

            migrationBuilder.InsertData(
                table: "Providers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Direct typing to terminal", "Keyboard" },
                    { 2, "ContinuousListener - voice input", "VoiceAssistant" },
                    { 3, "Combination of keyboard and voice", "HumanCombination" },
                    { 4, "Claude models via Anthropic API", "Anthropic" },
                    { 5, "GPT models via OpenAI API", "OpenAI" },
                    { 6, "GitHub Copilot (routes to various models)", "GitHubCopilot" },
                    { 7, "Gemini models via Google AI", "Google" },
                    { 8, "Azure OpenAI Service", "AzureOpenAI" },
                    { 9, "Grok models via xAI", "xAI" }
                });

            migrationBuilder.InsertData(
                table: "Participants",
                columns: new[] { "Id", "Identifier", "Label", "ParticipantTypeId" },
                values: new object[,]
                {
                    { 1, "user-jirka", "Jirka", 1 },
                    { 2, "claude-haiku-4.5", "Claude Haiku 4.5", 2 },
                    { 3, "claude-opus-4.1", "Claude Opus 4.1", 2 },
                    { 4, "claude-opus-4.5", "Claude Opus 4.5", 2 },
                    { 5, "claude-sonnet-4", "Claude Sonnet 4", 2 },
                    { 6, "claude-sonnet-4.5", "Claude Sonnet 4.5", 2 },
                    { 7, "gpt-4.1", "GPT-4.1", 2 },
                    { 8, "gpt-5", "GPT-5", 2 },
                    { 9, "gpt-5-mini", "GPT-5 mini", 2 },
                    { 10, "gpt-5-codex", "GPT-5-Codex", 2 },
                    { 11, "gpt-5.1", "GPT-5.1", 2 },
                    { 12, "gpt-5.1-codex", "GPT-5.1-Codex", 2 },
                    { 13, "gpt-5.1-codex-mini", "GPT-5.1-Codex-Mini", 2 },
                    { 14, "raptor-mini", "Raptor mini", 2 },
                    { 15, "gemini-2.5-pro", "Gemini 2.5 Pro", 2 },
                    { 16, "gemini-3-pro", "Gemini 3 Pro", 2 },
                    { 17, "grok-code-fast-1", "Grok Code Fast 1", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageId",
                table: "Messages",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ModeId",
                table: "Messages",
                column: "ModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParticipantId",
                table: "Messages",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ProviderId",
                table: "Messages",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId",
                table: "Messages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Modes_Name",
                table: "Modes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_Identifier",
                table: "Participants",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ParticipantTypeId",
                table: "Participants",
                column: "ParticipantTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantTypes_Name",
                table: "ParticipantTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Name",
                table: "Providers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionId",
                table: "Sessions",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "ParticipantTypes");
        }
    }
}
