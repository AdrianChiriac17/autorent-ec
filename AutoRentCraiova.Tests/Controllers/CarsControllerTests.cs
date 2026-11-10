using AutoRentCraiova.Controllers;
using AutoRentCraiova.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoRentCraiova.Tests.Controllers
{
    public class CarsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);
            
            dbContext.Categories.Add(new Category { Id = 1, Name = "Sedan" });
            dbContext.Categories.Add(new Category { Id = 2, Name = "SUV" });
            
            // adaugare prețuri distincte pentru a putea testa sortarea
            dbContext.Cars.AddRange(
                new Car { Id = 1, Brand = "Dacia", Model = "Logan", Year = 2022, PricePerDay = 100, CategoryId = 1, VIN = "VIN1", LicensePlate = "B11AAA", Status = CarStatus.Disponibila },
                new Car { Id = 2, Brand = "BMW", Model = "X5", Year = 2023, PricePerDay = 300, CategoryId = 2, VIN = "VIN2", LicensePlate = "B22BBB", Status = CarStatus.Disponibila },
                new Car { Id = 3, Brand = "Audi", Model = "A4", Year = 2021, PricePerDay = 200, CategoryId = 1, VIN = "VIN3", LicensePlate = "B33CCC", Status = CarStatus.Disponibila }
            );

            dbContext.SaveChanges();
            return dbContext;
        }

        [Fact] // testul 4
        public async Task LoadMoreCars_WithSearchString_ReturnsFilteredCars()
        {
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);

            var result = await controller.LoadMoreCars(page: 0, searchString: "dacia", null, null, null, null, null) as PartialViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Car>>(result.Model);
            Assert.Single(model);
            Assert.Equal("Dacia", model[0].Brand);
        }

        [Fact] // testul 5
        public async Task LoadMoreCars_WithCategoryId_ReturnsFilteredCars()
        {
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);

            var result = await controller.LoadMoreCars(page: 0, searchString: null, categoryId: 2, null, null, null, null) as PartialViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Car>>(result.Model);
            Assert.Single(model);
            Assert.Equal("BMW", model[0].Brand);
        }

        [Fact] // testul 6: verifica algoritmul de sortare crescatoare a pretului
        public async Task LoadMoreCars_SortByPriceAsc_ReturnsCorrectOrder()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);

            // act - sortam ascendent
            var result = await controller.LoadMoreCars(page: 0, null, null, null, null, null, sortOrder: "price_asc") as PartialViewResult;

            // assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Car>>(result.Model);
            
            // trebuie sa returneze toate cele 3 masini
            Assert.Equal(3, model.Count);
            
            // ordinea corecta (100, 200, 300) -> dacia, audi, bmw
            Assert.Equal("Dacia", model[0].Brand);
            Assert.Equal("Audi", model[1].Brand);
            Assert.Equal("BMW", model[2].Brand);
        }

        [Fact] // testul 7: verifica algoritmul de sortare descrescatoare a pretului
        public async Task LoadMoreCars_SortByPriceDesc_ReturnsCorrectOrder()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);

            // act - sortam descendent
            var result = await controller.LoadMoreCars(page: 0, null, null, null, null, null, sortOrder: "price_desc") as PartialViewResult;

            // assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Car>>(result.Model);
            
            Assert.Equal(3, model.Count);
            
            // ordinea corecta 
            Assert.Equal("BMW", model[0].Brand);
            Assert.Equal("Audi", model[1].Brand);
            Assert.Equal("Dacia", model[2].Brand);
        }

        [Fact] // testul 8: verifica daca actiunea de delete sterge masina din db
        public async Task DeleteConfirmed_WithValidId_DeletesCar()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);
            var carToDeleteId = 2; // stergem bmw-ul

            // act
            var result = await controller.DeleteConfirmed(carToDeleteId) as JsonResult;

            // assert
            Assert.NotNull(result);
            // controller-ul returneaza json cu { success = true } cand masina e stearsa
            var successProperty = result.Value.GetType().GetProperty("success");
            Assert.True((bool)successProperty.GetValue(result.Value, null));
            
            // verificam ca masina a disparut din "baza de date"
            var carInDb = await dbContext.Cars.FindAsync(carToDeleteId);
            Assert.Null(carInDb);
            Assert.Equal(2, await dbContext.Cars.CountAsync()); // initial erau 3, acum trebuie sa fie 2
        }

        [Fact] // testul 9: verifica ce se intampla cand incerci sa stergi o masina care nu exista
        public async Task DeleteConfirmed_WithInvalidId_ReturnsFailure()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);
            var invalidCarId = 999;

            // act
            var result = await controller.DeleteConfirmed(invalidCarId) as JsonResult;

            // assert
            Assert.NotNull(result);
            // controller-ul returneaza json { success = false, message = "car not found" }
            var successProperty = result.Value.GetType().GetProperty("success");
            Assert.False((bool)successProperty.GetValue(result.Value, null));
            
            // ne asiguram ca nicio masina nu a fost stearsa
            Assert.Equal(3, await dbContext.Cars.CountAsync());
        }

        [Fact] // testul 11: verifica algoritmul de schimbare a statusului unei masini
        public async Task ChangeStatus_WithValidId_UpdatesCarStatus()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);
            var carToUpdateId = 1; // luam dacia (id 1) care e 'Disponibila'
            var newStatus = CarStatus.Inchiriata;

            // act
            var result = await controller.ChangeStatus(carToUpdateId, newStatus) as JsonResult;

            // assert
            Assert.NotNull(result);
            var successProperty = result.Value.GetType().GetProperty("success");
            Assert.True((bool)successProperty.GetValue(result.Value, null));

            // verificam in baza de date falsa ca statusul s-a schimbat
            var updatedCar = await dbContext.Cars.FindAsync(carToUpdateId);
            Assert.Equal(CarStatus.Inchiriata, updatedCar.Status);
        }

        [Fact] // testul 15: verifica daca actiunea details returneaza eroare pentru id invalid
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);
            var invalidCarId = 999; // id care nu exista

            // act
            var result = await controller.Details(invalidCarId);

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact] // testul 16: verifica functia de mentenanta pentru imagini
        public async Task FixImages_ForCarWithNullImageUrl_SetsDefaultImageUrl()
        {
            // arrange
            var dbContext = GetInMemoryDbContext();
            var carWithNoImage = new Car { Id = 4, Brand = "Test", Model = "TestModel", ImageUrl = null, CategoryId = 1, VIN = "VIN4", LicensePlate = "B44DDD" };
            dbContext.Cars.Add(carWithNoImage);
            await dbContext.SaveChangesAsync();

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var controller = new CarsController(dbContext, mockEnvironment.Object);

            // act
            await controller.FixImages();

            // assert
            var updatedCar = await dbContext.Cars.FindAsync(4);
            Assert.NotNull(updatedCar.ImageUrl);
            Assert.Equal("testmodel.png", updatedCar.ImageUrl);
        }
    }
}