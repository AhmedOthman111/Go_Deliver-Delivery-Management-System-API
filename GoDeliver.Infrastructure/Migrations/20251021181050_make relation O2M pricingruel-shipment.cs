using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoDeliver.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class makerelationO2Mpricingruelshipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_PricingRuleId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PricingRuleId",
                table: "Shipments",
                column: "PricingRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_PricingRuleId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PricingRuleId",
                table: "Shipments",
                column: "PricingRuleId",
                unique: true,
                filter: "[PricingRuleId] IS NOT NULL");
        }
    }
}
