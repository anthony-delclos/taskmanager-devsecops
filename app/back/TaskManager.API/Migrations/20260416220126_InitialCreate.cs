using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(newid())", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_categ__3213E83F8AD1C9F5", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_customer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(newid())", collation: "ascii_general_ci"),
                    company_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    annual_allocated_load_days = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    status = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_custo__3213E83FD627A2CB", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(newid())", collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_admin = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_user__3213E83F52BF5AD0", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CmCategoryCmGroup",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmCategoryCmGroup", x => new { x.CategoryId, x.GroupId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CmCustomerCmGroup",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmCustomerCmGroup", x => new { x.CustomerId, x.GroupId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(newid())", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    edition_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_group__3213E83FBA5438E2", x => x.id);
                    table.ForeignKey(
                        name: "FK__cm_group__create__2A4B4B5E",
                        column: x => x.created_by_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    token_hash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    revoked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK__cm_refresh_token__user",
                        column: x => x.user_id,
                        principalTable: "cm_user",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_subject",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(newid())", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", unicode: false, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    priority = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estimated_load_hours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    actual_load_hours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    edition_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    category_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    assigned_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_subje__3213E83F35FFC490", x => x.id);
                    table.ForeignKey(
                        name: "FK__cm_subjec__assig__37A5467C",
                        column: x => x.assigned_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__cm_subjec__categ__35BCFE0A",
                        column: x => x.category_id,
                        principalTable: "cm_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__cm_subjec__creat__38996AB5",
                        column: x => x.created_by_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__cm_subjec__custo__36B12243",
                        column: x => x.customer_id,
                        principalTable: "cm_customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__cm_subjec__group__398D8EEE",
                        column: x => x.group_id,
                        principalTable: "cm_group",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GROUP_HAS_CATEGORY",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    category_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GROUP_HA__88237B3BFE958F51", x => new { x.group_id, x.category_id });
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__categ__44FF419A",
                        column: x => x.category_id,
                        principalTable: "cm_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__group__440B1D61",
                        column: x => x.group_id,
                        principalTable: "cm_group",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GROUP_HAS_CUSTOMER",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GROUP_HA__99A1C9188F4A2A6F", x => new { x.group_id, x.customer_id });
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__custo__412EB0B6",
                        column: x => x.customer_id,
                        principalTable: "cm_customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__group__403A8C7D",
                        column: x => x.group_id,
                        principalTable: "cm_group",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GROUP_HAS_USER",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    group_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_admin = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GROUP_HA__A4E94E55281D401F", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__group__48CFD27E",
                        column: x => x.group_id,
                        principalTable: "cm_group",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__GROUP_HAS__user___47DBAE45",
                        column: x => x.user_id,
                        principalTable: "cm_user",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cm_comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    creation_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    edition_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    content = table.Column<string>(type: "longtext", unicode: false, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    subject_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    status = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cm_comme__3213E83FB46C06D5", x => x.id);
                    table.ForeignKey(
                        name: "FK__cm_commen__creat__3C69FB99",
                        column: x => x.created_by_user_id,
                        principalTable: "cm_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__cm_commen__subje__3D5E1FD2",
                        column: x => x.subject_id,
                        principalTable: "cm_subject",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cm_comment_created_by_user_id",
                table: "cm_comment",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_comment_subject_id",
                table: "cm_comment",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_group_created_by_user_id",
                table: "cm_group",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_refresh_token_user_id",
                table: "cm_refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_cm_refresh_token_hash",
                table: "cm_refresh_token",
                column: "token_hash",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_cm_subject_customer_id",
                table: "cm_subject",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_cm_subject_group_id",
                table: "cm_subject",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_GROUP_HAS_CATEGORY_category_id",
                table: "GROUP_HAS_CATEGORY",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_GROUP_HAS_CUSTOMER_customer_id",
                table: "GROUP_HAS_CUSTOMER",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_GROUP_HAS_USER_group_id",
                table: "GROUP_HAS_USER",
                column: "group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cm_comment");

            migrationBuilder.DropTable(
                name: "cm_refresh_token");

            migrationBuilder.DropTable(
                name: "CmCategoryCmGroup");

            migrationBuilder.DropTable(
                name: "CmCustomerCmGroup");

            migrationBuilder.DropTable(
                name: "GROUP_HAS_CATEGORY");

            migrationBuilder.DropTable(
                name: "GROUP_HAS_CUSTOMER");

            migrationBuilder.DropTable(
                name: "GROUP_HAS_USER");

            migrationBuilder.DropTable(
                name: "cm_subject");

            migrationBuilder.DropTable(
                name: "cm_category");

            migrationBuilder.DropTable(
                name: "cm_customer");

            migrationBuilder.DropTable(
                name: "cm_group");

            migrationBuilder.DropTable(
                name: "cm_user");
        }
    }
}
