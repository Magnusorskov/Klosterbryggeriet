using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAndColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceLabel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoryColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayLabel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryColumns_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryColumns_CategoryId",
                table: "CategoryColumns",
                column: "CategoryId");

            // ----------------------------------------------------------------
            // Seed: 31 categories
            // ----------------------------------------------------------------
            // Layout: Gavekasser (no extra columns)
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [1,  "GAVEKASSER",                                    "pr. stk."]);

            // Layout: Saft (extra: KegCollar/Antal, Str)
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [2,  "MOSTERS økologiske saft - DANMARK",              "pr. stk."]);

            // Layout: Standard (extra: KegCollar, Str, Alkohol)
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [3,  "KLOSTERBRYGGERIET - DANMARK",                   "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [4,  "BOLSKOV CIDER & MOST",                           "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [5,  "ARDÉCHE likører - FRANKRIG",                     "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [6,  "3 Monts Biere de Flandern - FRANKRIG",           "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [7,  "AMARCORD - ITALIEN",                             "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [8,  "GALVANINA CENTURY LINE - ITALIEN",               "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [9,  "RIVIERA GIN - Premium Elitist - Italien",        "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [10, "CANEDIGUERRA - Italien",                         "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [11, "PRIMATOR - TJEKKIET",                            "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [12, "AYINGER - TYSKLAND",                             "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [13, "WESTVLETEREN - BELGIEN",                         "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [14, "HET ANKER BROUWERIJ - BELGIEN",                  "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [15, "HET ANKER STOKERIJ - BELGIEN",                   "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [16, "BATTELIEK - BELGIEN",                            "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [17, "SINT BERNARDUS - BELGIEN",                       "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [18, "BOON - BELGIEN",                                 "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [19, "DUBUISSON - BELGIEN",                            "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [20, "MARTHA - BELGIEN",                               "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [21, "7-ZONDERN - BELGIEN",                            "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [22, "TER DOLEN - BELGIEN",                            "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [23, "STADSHAVEN - HOLLAND",                           "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [24, "VAN STEENBERGE - BELGIEN",                       "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [25, "ORVAL TRAPPIST - BELGIEN",                       "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [26, "ACHEL TRAPPIST - BELGIEN",                       "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [27, "ROCHEFORT TRAPPIST - BELGIEN",                   "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [28, "WESTMALLE TRAPPIST - BELGIEN",                   "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [29, "CHOUFFE - BELGIEN",                              "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [30, "EGGENBERG - ØSTRIG",                             "pr. enhed"]);
            migrationBuilder.InsertData("Categories", ["Id", "Name", "PriceLabel"], [31, "Øvrige",                                         "pr. enhed"]);

            // ----------------------------------------------------------------
            // Seed: category columns
            // Col ID 1-2: Saft (catId=2) — KegCollar=Antal, Str
            // ----------------------------------------------------------------
            migrationBuilder.InsertData("CategoryColumns", ["Id", "CategoryId", "FieldName", "DisplayLabel", "SortOrder"],
                [1, 2, "KegCollar", "Antal", 1]);
            migrationBuilder.InsertData("CategoryColumns", ["Id", "CategoryId", "FieldName", "DisplayLabel", "SortOrder"],
                [2, 2, "Str", "Str.", 2]);

            // Standard categories (ids 3-31): KegCollar, Str, Alcohol
            int colId = 3;
            int[] standardCatIds = [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31];
            foreach (var catId in standardCatIds)
            {
                migrationBuilder.InsertData("CategoryColumns", ["Id", "CategoryId", "FieldName", "DisplayLabel", "SortOrder"],
                    [colId++, catId, "KegCollar", "Kasse/Kolli", 1]);
                migrationBuilder.InsertData("CategoryColumns", ["Id", "CategoryId", "FieldName", "DisplayLabel", "SortOrder"],
                    [colId++, catId, "Str", "Str.", 2]);
                migrationBuilder.InsertData("CategoryColumns", ["Id", "CategoryId", "FieldName", "DisplayLabel", "SortOrder"],
                    [colId++, catId, "Alcohol", "Alkohol%", 3]);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CategoryColumns");
            migrationBuilder.DropTable(name: "Categories");
        }
    }
}