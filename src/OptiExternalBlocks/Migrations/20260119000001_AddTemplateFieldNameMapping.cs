using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiExternalBlocks.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateFieldNameMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleFieldName",
                table: "tbl_OptiExternalBlocks_Templates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFieldName",
                table: "tbl_OptiExternalBlocks_Templates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitleFieldName",
                table: "tbl_OptiExternalBlocks_Templates");

            migrationBuilder.DropColumn(
                name: "ThumbnailFieldName",
                table: "tbl_OptiExternalBlocks_Templates");
        }
    }
}
