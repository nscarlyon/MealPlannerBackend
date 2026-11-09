using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictDeleteToMealIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealIngredient_Ingredients_IngredientId",
                table: "MealIngredient");

            migrationBuilder.AddForeignKey(
                name: "FK_MealIngredient_Ingredients_IngredientId",
                table: "MealIngredient",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealIngredient_Ingredients_IngredientId",
                table: "MealIngredient");

            migrationBuilder.AddForeignKey(
                name: "FK_MealIngredient_Ingredients_IngredientId",
                table: "MealIngredient",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
