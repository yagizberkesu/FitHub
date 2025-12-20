using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitHub.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Uyeler",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "Randevular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Uyeler",
                columns: new[] { "Id", "Ad", "Cinsiyet", "DogumTarihi", "Email", "Rol", "Sifre", "Soyad", "Telefon" },
                values: new object[] { 1, "Admin", "Diğer", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ogrencinumarasi@sakarya.edu.tr", "Admin", "sau", "Kullanıcı", "0000000000" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Uyeler",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Randevular");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Uyeler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
