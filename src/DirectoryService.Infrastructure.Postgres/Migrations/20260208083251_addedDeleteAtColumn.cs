using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedDeleteAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "positions",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "locations",
                newName: "deleted_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "positions",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "locations",
                newName: "DeletedAt");
        }
    }
}
