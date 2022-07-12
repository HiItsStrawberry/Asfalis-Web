using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asfalis.Server.DataContext.Migrations
{
    public partial class Fix_Image_Table_Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_image_iamges_image_id",
                table: "user_image");

            migrationBuilder.DropPrimaryKey(
                name: "PK_iamges",
                table: "iamges");

            migrationBuilder.RenameTable(
                name: "iamges",
                newName: "images");

            migrationBuilder.AddPrimaryKey(
                name: "PK_images",
                table: "images",
                column: "image_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_image_images_image_id",
                table: "user_image",
                column: "image_id",
                principalTable: "images",
                principalColumn: "image_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_image_images_image_id",
                table: "user_image");

            migrationBuilder.DropPrimaryKey(
                name: "PK_images",
                table: "images");

            migrationBuilder.RenameTable(
                name: "images",
                newName: "iamges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_iamges",
                table: "iamges",
                column: "image_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_image_iamges_image_id",
                table: "user_image",
                column: "image_id",
                principalTable: "iamges",
                principalColumn: "image_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
