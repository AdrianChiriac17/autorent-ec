using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoRentCraiova.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentsStatus",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentsStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DriverLicenseUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdCardUrl",
                table: "Users");
        }
    }
}
