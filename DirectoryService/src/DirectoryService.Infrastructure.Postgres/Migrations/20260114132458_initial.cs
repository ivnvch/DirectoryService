using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    identifier = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_departments_parent_id",
                        column: x => x.parent_id,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_location_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_position_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "department_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_department_location_department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_department_location_location_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_location_id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_department_locations_departments_fk_department_location_dep~",
                        column: x => x.fk_department_location_department_id,
                        principalTable: "departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_department_locations_locations_fk_department_location_locat~",
                        column: x => x.fk_department_location_location_id,
                        principalTable: "locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "department_position",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_department_position_department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_department_position_position_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department_position_id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_department_position_departments_fk_department_position_depa~",
                        column: x => x.fk_department_position_department_id,
                        principalTable: "departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_department_position_positions_fk_department_position_positi~",
                        column: x => x.fk_department_position_position_id,
                        principalTable: "positions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_fk_department_location_department_id",
                table: "department_locations",
                column: "fk_department_location_department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_fk_department_location_location_id",
                table: "department_locations",
                column: "fk_department_location_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_position_fk_department_position_department_id",
                table: "department_position",
                column: "fk_department_position_department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_position_fk_department_position_position_id",
                table: "department_position",
                column: "fk_department_position_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_departments_parent_id",
                table: "departments",
                column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "department_locations");

            migrationBuilder.DropTable(
                name: "department_position");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
