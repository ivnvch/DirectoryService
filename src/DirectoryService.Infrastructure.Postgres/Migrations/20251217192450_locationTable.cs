using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class locationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_LocationId1",
                table: "department_locations");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_LocationId1",
                table: "department_locations");

            migrationBuilder.DropColumn(
                name: "LocationId1",
                table: "department_locations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId1",
                table: "department_locations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_LocationId1",
                table: "department_locations",
                column: "LocationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_LocationId1",
                table: "department_locations",
                column: "LocationId1",
                principalTable: "locations",
                principalColumn: "Id");
        }
    }
}
