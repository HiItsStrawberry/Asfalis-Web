using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asfalis.Server.DataContext.Migrations
{
    public partial class Update_QRCode_Column_Code_Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "code",
                table: "qr_codes",
                newName: "otp_code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "otp_code",
                table: "qr_codes",
                newName: "code");
        }
    }
}
