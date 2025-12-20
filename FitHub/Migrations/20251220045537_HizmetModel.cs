using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitHub.Migrations
{
    /// <inheritdoc />
    public partial class HizmetModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Egitmenler_EgitmenId",
                table: "Randevular");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Uyeler_UyeId",
                table: "Randevular");

            migrationBuilder.AddColumn<int>(
                name: "HizmetId",
                table: "Randevular",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Hizmetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SureDakika = table.Column<int>(type: "int", nullable: false),
                    Ucret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hizmetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hizmetler_Salonlar_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EgitmenHizmetler",
                columns: table => new
                {
                    EgitmenId = table.Column<int>(type: "int", nullable: false),
                    HizmetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgitmenHizmetler", x => new { x.EgitmenId, x.HizmetId });
                    table.ForeignKey(
                        name: "FK_EgitmenHizmetler_Egitmenler_EgitmenId",
                        column: x => x.EgitmenId,
                        principalTable: "Egitmenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EgitmenHizmetler_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Uyeler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cinsiyet", "Soyad" },
                values: new object[] { "Diger", "Kullanici" });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HizmetId",
                table: "Randevular",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_EgitmenHizmetler_HizmetId",
                table: "EgitmenHizmetler",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_SalonId",
                table: "Hizmetler",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Egitmenler_EgitmenId",
                table: "Randevular",
                column: "EgitmenId",
                principalTable: "Egitmenler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Hizmetler_HizmetId",
                table: "Randevular",
                column: "HizmetId",
                principalTable: "Hizmetler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Uyeler_UyeId",
                table: "Randevular",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Egitmenler_EgitmenId",
                table: "Randevular");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Hizmetler_HizmetId",
                table: "Randevular");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Uyeler_UyeId",
                table: "Randevular");

            migrationBuilder.DropTable(
                name: "EgitmenHizmetler");

            migrationBuilder.DropTable(
                name: "Hizmetler");

            migrationBuilder.DropIndex(
                name: "IX_Randevular_HizmetId",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "HizmetId",
                table: "Randevular");

            migrationBuilder.UpdateData(
                table: "Uyeler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cinsiyet", "Soyad" },
                values: new object[] { "Diğer", "Kullanıcı" });

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Egitmenler_EgitmenId",
                table: "Randevular",
                column: "EgitmenId",
                principalTable: "Egitmenler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Uyeler_UyeId",
                table: "Randevular",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
