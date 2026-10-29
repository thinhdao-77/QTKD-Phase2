using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QTKD.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaxCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "price_lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContentSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contracts_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PriceListId = table.Column<int>(type: "integer", nullable: false),
                    ServiceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_price_items_price_lists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "price_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "customers",
                columns: new[] { "Id", "Address", "ContactEmail", "CreatedAt", "CustomerCode", "Name", "Status", "TaxCode" },
                values: new object[,]
                {
                    { 1, "123 Nguyễn Văn Linh, Quận 7, TP.HCM", "contact@minhphatlogistics.vn", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "KH-001", "Công ty TNHH Logistics Minh Phát", "Active", "0312345678" },
                    { 2, "45 Lê Thánh Tôn, Quận 1, TP.HCM", "info@biendongshipping.vn", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "KH-002", "Công ty Cổ phần Vận tải Biển Đông", "Active", "0398765432" }
                });

            migrationBuilder.InsertData(
                table: "price_lists",
                columns: new[] { "Id", "CreatedAt", "EffectiveDate", "ExpiryDate", "Name", "Status" },
                values: new object[] { 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "Bảng giá dịch vụ khai thác cảng tiêu chuẩn 2026", "Effective" });

            migrationBuilder.InsertData(
                table: "contracts",
                columns: new[] { "Id", "AttachmentUrl", "ContentSummary", "ContractNo", "CreatedAt", "CustomerId", "EndDate", "ServiceGroup", "StartDate", "Status", "SubmittedAt" },
                values: new object[,]
                {
                    { 1, "https://storage.qtkd.local/contracts/HD-2026-018.pdf", "Hợp đồng cung cấp dịch vụ bốc xếp và lưu kho bãi container tại cảng Cát Lái", "HD-2026-018", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateOnly(2027, 9, 30), "Dịch vụ cảng", new DateOnly(2026, 10, 1), "Draft", null },
                    { 2, null, null, "HD-2026-019", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateOnly(2027, 10, 31), "Vận chuyển nội địa", new DateOnly(2026, 11, 1), "Draft", null }
                });

            migrationBuilder.InsertData(
                table: "price_items",
                columns: new[] { "Id", "PriceListId", "ServiceCode", "ServiceName", "Unit", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, "SVC-CONT-20", "Bốc xếp container 20 feet", "Container", 1250000m },
                    { 2, 1, "SVC-STORAGE-20", "Lưu kho bãi container 20 feet", "Ngày", 180000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_ContractNo",
                table: "contracts",
                column: "ContractNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contracts_CustomerId",
                table: "contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_CustomerCode",
                table: "customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_price_items_PriceListId_ServiceCode",
                table: "price_items",
                columns: new[] { "PriceListId", "ServiceCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "price_items");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "price_lists");
        }
    }
}
