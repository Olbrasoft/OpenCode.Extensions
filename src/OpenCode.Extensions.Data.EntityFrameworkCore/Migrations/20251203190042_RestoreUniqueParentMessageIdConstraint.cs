using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RestoreUniqueParentMessageIdConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Restore the unique constraint on (SessionId, ParentMessageId)
            // This ensures each message can only have one child (linear chain)
            // Note: This was manually removed earlier, now restoring it
            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId_ParentMessageId_Unique",
                table: "Messages",
                columns: new[] { "SessionId", "ParentMessageId" },
                unique: true,
                filter: "\"ParentMessageId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_SessionId_ParentMessageId_Unique",
                table: "Messages");
        }
    }
}
