using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueParentMessagePerSessionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
