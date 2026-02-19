using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class edit_names : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_DepartmentId",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_LocationId",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_departments_DepartmentId",
                table: "department_position");

            migrationBuilder.DropForeignKey(
                name: "FK_department_position_positions_PositionId",
                table: "department_position");

            migrationBuilder.DropPrimaryKey(
                name: "department_position_department_id",
                table: "department_position");

            migrationBuilder.DropPrimaryKey(
                name: "department_location_id",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "department_position",
                newName: "fk_department_position_position_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_position",
                newName: "fk_department_position_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_PositionId",
                table: "department_position",
                newName: "IX_department_position_fk_department_position_position_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "department_locations",
                newName: "fk_department_location_location_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_locations",
                newName: "fk_department_location_department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_LocationId",
                table: "department_locations",
                newName: "IX_department_locations_fk_department_location_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_DepartmentId",
                table: "department_locations",
                newName: "IX_department_locations_fk_department_location_department_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_position_id",
                table: "department_position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_location_id",
                table: "department_locations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_department_position_fk_department_position_department_id",
                table: "department_position",
                column: "fk_department_position_department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_fk_department_location_dep~",
                table: "department_locations",
                column: "fk_department_location_department_id",
                principalTable: "departments",
                principalColumn: "Id");

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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_positions_fk_department_position_positi~",
                table: "department_position",
                column: "fk_department_position_position_id",
                principalTable: "positions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_position_id",
                table: "department_position");

            migrationBuilder.DropIndex(
                name: "IX_department_position_fk_department_position_department_id",
                table: "department_position");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_location_id",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "fk_department_position_position_id",
                table: "department_position",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "fk_department_position_department_id",
                table: "department_position",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_department_position_fk_department_position_position_id",
                table: "department_position",
                newName: "IX_department_position_PositionId");

            migrationBuilder.RenameColumn(
                name: "fk_department_location_location_id",
                table: "department_locations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "fk_department_location_department_id",
                table: "department_locations",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_fk_department_location_location_id",
                table: "department_locations",
                newName: "IX_department_locations_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_fk_department_location_department_id",
                table: "department_locations",
                newName: "IX_department_locations_DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "department_position_department_id",
                table: "department_position",
                column: "DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "department_location_id",
                table: "department_locations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_DepartmentId",
                table: "department_locations",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_LocationId",
                table: "department_locations",
                column: "LocationId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_departments_DepartmentId",
                table: "department_position",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_position_positions_PositionId",
                table: "department_position",
                column: "PositionId",
                principalTable: "positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
