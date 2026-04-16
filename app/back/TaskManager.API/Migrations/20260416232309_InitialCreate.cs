using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManager.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_category",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "UUID()", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cm_category", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "UUID()", collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cm_user", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_subject",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "UUID()", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    priority = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estimated_load_hours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    actual_load_hours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    edition_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    category_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    assigned_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cm_subject", x => x.id);
                    table.ForeignKey(
                        name: "FK_cm_subject_cm_category_category_id",
                        column: x => x.category_id,
                        principalTable: "cm_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cm_subject_cm_user_assigned_user_id",
                        column: x => x.assigned_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cm_subject_cm_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "cm_category",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("150ab93d-d6a1-47c2-bff4-9fa6f0998a06"), "Etude" },
                    { new Guid("1f170089-7223-4f5a-9902-b0354fbe4e7a"), "Support" },
                    { new Guid("236b71aa-9ef4-4e06-b951-6598062aa199"), "Debug" },
                    { new Guid("57f93148-b137-4324-91cd-702dcdb7d562"), "Optimisation" },
                    { new Guid("fdc5572c-bec1-4b7e-8efc-03fd3e1a2776"), "Evolution" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_cm_subject_assigned_user_id",
                table: "cm_subject",
                column: "assigned_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_subject_category_id",
                table: "cm_subject",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_subject_created_by_user_id",
                table: "cm_subject",
                column: "created_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cm_subject");

            migrationBuilder.DropTable(
                name: "cm_category");

            migrationBuilder.DropTable(
                name: "cm_user");
        }
    }
}
