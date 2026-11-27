using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bastina3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Sites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Rank",
                table: "Sites",
                column: "Rank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_Rank",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Sites");
        }
    }
}
