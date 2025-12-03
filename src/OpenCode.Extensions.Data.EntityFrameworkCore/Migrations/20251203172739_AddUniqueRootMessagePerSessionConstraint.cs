using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRootMessagePerSessionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_SessionId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId_RootMessage",
                table: "Messages",
                column: "SessionId",
                unique: true,
                filter: "\"ParentMessageId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_SessionId_RootMessage",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId",
                table: "Messages",
                column: "SessionId");
        }
    }
}
