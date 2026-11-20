using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoRentCraiova.Migrations
{
    /// <inheritdoc />
    public partial class AddCarStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Am comentat adaugarea coloanei Status deoarece exista deja in baza de date si dadea eroare la database update
            /*
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cars");
            */
        }
    }
}