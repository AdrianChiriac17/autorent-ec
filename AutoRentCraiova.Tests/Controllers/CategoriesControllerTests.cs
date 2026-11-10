using AutoRentCraiova.Controllers;
using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoRentCraiova.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            return dbContext;
        }

        [Fact] // testul 10: verifica crearea unei noi categorii
        public async Task Create_Post_AddsNewCategoryToDatabase()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new CategoriesController(dbContext);
            var newCategory = new Category { Name = "Test Category" };

            // act
            await controller.Create(newCategory);

            // assert
            var categoryInDb = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Test Category");
            Assert.NotNull(categoryInDb);
            Assert.Equal("Test Category", categoryInDb.Name);
        }
    }
}