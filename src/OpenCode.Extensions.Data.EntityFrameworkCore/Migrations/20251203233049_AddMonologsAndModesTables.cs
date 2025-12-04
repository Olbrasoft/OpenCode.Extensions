using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddMonologsAndModesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create pgvector extension if not exists
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            // Create Monologs table
            migrationBuilder.CreateTable(
                name: "Monologs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    ParentMonologId = table.Column<int>(type: "integer", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    FirstMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Pgvector.Vector>(type: "vector(1536)", nullable: true),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    ModeId = table.Column<int>(type: "integer", nullable: false),
                    TokensInput = table.Column<int>(type: "integer", nullable: true),
                    TokensOutput = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsAborted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monologs", x => x.Id);
                    table.CheckConstraint("CHK_Assistant_Has_Parent", "\"Role\" = 1 OR \"ParentMonologId\" IS NOT NULL");
                    table.CheckConstraint("CHK_Closed_Has_LastMessageId", "\"CompletedAt\" IS NULL OR \"LastMessageId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Monologs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Monologs_Monologs_ParentMonologId",
                        column: x => x.ParentMonologId,
                        principalTable: "Monologs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Monologs_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Monologs_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Monologs_Modes_ModeId",
                        column: x => x.ModeId,
                        principalTable: "Modes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LegacyMonologs table (no constraints, accepts any data)
            migrationBuilder.CreateTable(
                name: "LegacyMonologs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentMonologId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    FirstMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Embedding = table.Column<Pgvector.Vector>(type: "vector(1536)", nullable: true),
                    ParticipantIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TokensInput = table.Column<int>(type: "integer", nullable: true),
                    TokensOutput = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsAborted = table.Column<bool>(type: "boolean", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    OriginalPayload = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyMonologs", x => x.Id);
                });

            // Add Description column to Modes if not exists
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'Modes' AND column_name = 'Description') THEN
                        ALTER TABLE ""Modes"" ADD COLUMN ""Description"" character varying(200);
                    END IF;
                END $$;
            ");

            // Update Modes descriptions
            migrationBuilder.Sql(@"
                UPDATE ""Modes"" SET ""Description"" = 'AI can modify files, run commands (full access)' WHERE ""Id"" = 1;
                UPDATE ""Modes"" SET ""Description"" = 'AI only suggests and plans (read-only)' WHERE ""Id"" = 2;
            ");

            // Indexes for Monologs
            migrationBuilder.CreateIndex(
                name: "IX_Monologs_SessionId",
                table: "Monologs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Monologs_ParentMonologId",
                table: "Monologs",
                column: "ParentMonologId");

            migrationBuilder.CreateIndex(
                name: "IX_Monologs_ParticipantId",
                table: "Monologs",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Monologs_ProviderId",
                table: "Monologs",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Monologs_ModeId",
                table: "Monologs",
                column: "ModeId");

            // Partial index for finding open monologs quickly
            migrationBuilder.CreateIndex(
                name: "IX_Monologs_Open",
                table: "Monologs",
                columns: new[] { "SessionId", "Role" },
                filter: "\"CompletedAt\" IS NULL");

            // Indexes for LegacyMonologs
            migrationBuilder.CreateIndex(
                name: "IX_LegacyMonologs_SessionId",
                table: "LegacyMonologs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyMonologs_FirstMessageId",
                table: "LegacyMonologs",
                column: "FirstMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyMonologs_CreatedAt",
                table: "LegacyMonologs",
                column: "CreatedAt");

            // HNSW index for fast vector similarity search (cosine distance)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Monologs_Embedding_HNSW"" 
                ON ""Monologs"" 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
                
                CREATE INDEX IF NOT EXISTS ""IX_LegacyMonologs_Embedding_HNSW"" 
                ON ""LegacyMonologs"" 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop HNSW indexes
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Monologs_Embedding_HNSW"";
                DROP INDEX IF EXISTS ""IX_LegacyMonologs_Embedding_HNSW"";
            ");

            migrationBuilder.DropTable(name: "LegacyMonologs");
            migrationBuilder.DropTable(name: "Monologs");
        }
    }
}
