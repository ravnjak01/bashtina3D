using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Bastina3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NameLocal = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YearBuilt = table.Column<int>(type: "int", nullable: true),
                    Municipality = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsNationalMonument = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    ElevationM = table.Column<double>(type: "float", nullable: true),
                    WikidataId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OsmType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OsmId = table.Column<long>(type: "bigint", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attribution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    License = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Scale = table.Column<double>(type: "float", nullable: false),
                    HeadingDeg = table.Column<double>(type: "float", nullable: false),
                    AltitudeOffsetM = table.Column<double>(type: "float", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_SiteId",
                table: "Media",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_SiteId",
                table: "Models",
                column: "SiteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Category",
                table: "Sites",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_OsmType_OsmId",
                table: "Sites",
                columns: new[] { "OsmType", "OsmId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Slug",
                table: "Sites",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_WikidataId",
                table: "Sites",
                column: "WikidataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
