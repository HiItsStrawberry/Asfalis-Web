using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asfalis.Server.DataContext.Migrations
{
    public partial class Update_Tables_Names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QRCodes_Users_user_id",
                table: "QRCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserImage_Images_image_id",
                table: "UserImage");

            migrationBuilder.DropForeignKey(
                name: "FK_UserImage_Users_user_id",
                table: "UserImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserImage",
                table: "UserImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QRCodes",
                table: "QRCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "UserImage",
                newName: "user_image");

            migrationBuilder.RenameTable(
                name: "QRCodes",
                newName: "qr_codes");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "iamges");

            migrationBuilder.RenameIndex(
                name: "IX_Users_username_email",
                table: "users",
                newName: "IX_users_username_email");

            migrationBuilder.RenameIndex(
                name: "IX_UserImage_image_id",
                table: "user_image",
                newName: "IX_user_image_image_id");

            migrationBuilder.RenameIndex(
                name: "IX_QRCodes_user_id",
                table: "qr_codes",
                newName: "IX_qr_codes_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_image",
                table: "user_image",
                columns: new[] { "user_id", "image_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_qr_codes",
                table: "qr_codes",
                column: "code_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_iamges",
                table: "iamges",
                column: "image_id");

            migrationBuilder.AddForeignKey(
                name: "FK_qr_codes_users_user_id",
                table: "qr_codes",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_image_iamges_image_id",
                table: "user_image",
                column: "image_id",
                principalTable: "iamges",
                principalColumn: "image_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_image_users_user_id",
                table: "user_image",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_qr_codes_users_user_id",
                table: "qr_codes");

            migrationBuilder.DropForeignKey(
                name: "FK_user_image_iamges_image_id",
                table: "user_image");

            migrationBuilder.DropForeignKey(
                name: "FK_user_image_users_user_id",
                table: "user_image");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_image",
                table: "user_image");

            migrationBuilder.DropPrimaryKey(
                name: "PK_qr_codes",
                table: "qr_codes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_iamges",
                table: "iamges");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "user_image",
                newName: "UserImage");

            migrationBuilder.RenameTable(
                name: "qr_codes",
                newName: "QRCodes");

            migrationBuilder.RenameTable(
                name: "iamges",
                newName: "Images");

            migrationBuilder.RenameIndex(
                name: "IX_users_username_email",
                table: "Users",
                newName: "IX_Users_username_email");

            migrationBuilder.RenameIndex(
                name: "IX_user_image_image_id",
                table: "UserImage",
                newName: "IX_UserImage_image_id");

            migrationBuilder.RenameIndex(
                name: "IX_qr_codes_user_id",
                table: "QRCodes",
                newName: "IX_QRCodes_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserImage",
                table: "UserImage",
                columns: new[] { "user_id", "image_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_QRCodes",
                table: "QRCodes",
                column: "code_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "image_id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodes_Users_user_id",
                table: "QRCodes",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserImage_Images_image_id",
                table: "UserImage",
                column: "image_id",
                principalTable: "Images",
                principalColumn: "image_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserImage_Users_user_id",
                table: "UserImage",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
