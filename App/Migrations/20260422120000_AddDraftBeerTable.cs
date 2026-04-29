using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftBeerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftBeers",
                columns: table => new
                {
                    OctopusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", Microsoft.EntityFrameworkCore.Metadata.MySqlValueGenerationStrategy.IdentityColumn),
                    WebId = table.Column<int>(type: "int", nullable: false),
                    WebTitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PdfTitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OctopusTitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Available = table.Column<int>(type: "int", nullable: false),
                    Str = table.Column<double>(type: "double", nullable: false),
                    Alcohol = table.Column<double>(type: "double", nullable: false),
                    PricePrUnit = table.Column<double>(type: "double", nullable: false),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VariantId1 = table.Column<int>(type: "int", nullable: false),
                    VariantId2 = table.Column<int>(type: "int", nullable: false),
                    InUse = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Kobling = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Land = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftBeers", x => x.OctopusId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Move existing draft-beer products (WebTitle matches \d+\s*L) into DraftBeers.
            migrationBuilder.Sql(@"
                INSERT INTO DraftBeers
                    (OctopusId, WebId, WebTitle, PdfTitle, OctopusTitle, Available,
                     Str, Alcohol, PricePrUnit, Category,
                     VariantId1, VariantId2, InUse, Kobling, Land)
                SELECT
                    OctopusId, WebId, WebTitle, PdfTitle, OctopusTitle, Available,
                    Str, Alcohol, PricePrUnit, Category,
                    VariantId1, VariantId2, InUse, '' AS Kobling, '' AS Land
                FROM Products
                WHERE WebTitle REGEXP '[0-9]+ *L';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM Products
                WHERE WebTitle REGEXP '[0-9]+ *L';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftBeers");
        }
    }
}