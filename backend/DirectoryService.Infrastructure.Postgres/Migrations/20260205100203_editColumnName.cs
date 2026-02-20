using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_fk_department_location_dep~",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_fk_department_location_locat~",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_departments_fk_department_position_depa~",
                table: "department_position");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_positions_fk_department_position_positi~",
                table: "department_position");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "department_position",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "fk_department_position_position_id",
                table: "department_position",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "fk_department_position_department_id",
                table: "department_position",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_fk_department_position_position_id",
                table: "department_position",
                newName: "IX_department_position_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_fk_department_position_department_id",
                table: "department_position",
                newName: "IX_department_position_department_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "department_locations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "fk_department_location_location_id",
                table: "department_locations",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "fk_department_location_department_id",
                table: "department_locations",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_fk_department_location_location_id",
                table: "department_locations",
                newName: "IX_department_locations_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_fk_department_location_department_id",
                table: "department_locations",
                newName: "IX_department_locations_department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_departments_department_id",
                table: "department_position",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_positions_position_id",
                table: "department_position",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_departments_department_id",
                table: "department_position");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_positions_position_id",
                table: "department_position");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "department_position",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "department_position",
                newName: "fk_department_position_position_id");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_position",
                newName: "fk_department_position_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_position_id",
                table: "department_position",
                newName: "IX_department_position_fk_department_position_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_department_id",
                table: "department_position",
                newName: "IX_department_position_fk_department_position_department_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "department_locations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "department_locations",
                newName: "fk_department_location_location_id");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_locations",
                newName: "fk_department_location_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations",
                newName: "IX_department_locations_fk_department_location_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                newName: "IX_department_locations_fk_department_location_department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_fk_department_location_dep~",
                table: "department_locations",
                column: "fk_department_location_department_id",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_fk_department_location_locat~",
                table: "department_locations",
                column: "fk_department_location_location_id",
                principalTable: "locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_departments_fk_department_position_depa~",
                table: "department_position",
                column: "fk_department_position_department_id",
                principalTable: "departments",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_positions_fk_department_position_positi~",
                table: "department_position",
                column: "fk_department_position_position_id",
                principalTable: "positions",
                principalColumn: "Id");
        }
    }
}
