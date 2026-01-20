using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiExternalBlocks.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_OptiExternalBlocks_Endpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EndpointUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    SingleKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AppKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AppSecret = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OptiExternalBlocks_Endpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_OptiExternalBlocks_Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentTypeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    EditModeTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenderTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GraphQLQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GraphQLVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OptiExternalBlocks_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_OptiExternalBlocks_References",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CachedTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CachedThumbnailUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CachedData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CacheUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OptiExternalBlocks_References", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_OptiExternalBlocks_References_tbl_OptiExternalBlocks_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "tbl_OptiExternalBlocks_Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_Endpoints_IsActive",
                table: "tbl_OptiExternalBlocks_Endpoints",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_Endpoints_IsDefault",
                table: "tbl_OptiExternalBlocks_Endpoints",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_References_ExternalId",
                table: "tbl_OptiExternalBlocks_References",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_References_TemplateId",
                table: "tbl_OptiExternalBlocks_References",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_Templates_ContentTypeName",
                table: "tbl_OptiExternalBlocks_Templates",
                column: "ContentTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OptiExternalBlocks_Templates_IsActive",
                table: "tbl_OptiExternalBlocks_Templates",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_OptiExternalBlocks_Endpoints");

            migrationBuilder.DropTable(
                name: "tbl_OptiExternalBlocks_References");

            migrationBuilder.DropTable(
                name: "tbl_OptiExternalBlocks_Templates");
        }
    }
}
