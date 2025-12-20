using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitHub.Migrations
{
    /// <inheritdoc />
    public partial class Slot30_Contenjan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalismaSaatleri",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "Saat",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "MusaitlikSaatleri",
                table: "Egitmenler");

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Salonlar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CalismaBaslangic",
                table: "Salonlar",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CalismaBitis",
                table: "Salonlar",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Kontenjan",
                table: "Salonlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Tarih",
                table: "Randevular",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Baslangic",
                table: "Randevular",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Bitis",
                table: "Randevular",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "KisiSayisi",
                table: "Randevular",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Egitmenler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "MusaitlikBaslangic",
                table: "Egitmenler",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "MusaitlikBitis",
                table: "Egitmenler",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalismaBaslangic",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "CalismaBitis",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "Kontenjan",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "Baslangic",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "Bitis",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "KisiSayisi",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "MusaitlikBaslangic",
                table: "Egitmenler");

            migrationBuilder.DropColumn(
                name: "MusaitlikBitis",
                table: "Egitmenler");

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "CalismaSaatleri",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "Randevular",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "Saat",
                table: "Randevular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Egitmenler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "MusaitlikSaatleri",
                table: "Egitmenler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
