using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoRentCraiova.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaracteristiciMasini : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageConsumption",
                table: "Cars",
                type: "decimal(4,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasAC",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Seats",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageConsumption",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "HasAC",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Seats",
                table: "Cars");
        }
    }
}
