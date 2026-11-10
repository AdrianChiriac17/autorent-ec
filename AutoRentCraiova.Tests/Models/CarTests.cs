using AutoRentCraiova.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Tests.Models
{
    public class CarTests
    {
        [Fact]  // verifica daca o masina nou creata apare direct cu status de disponibila
        public void CreateCar_DefaultStatus_IsDisponibila()
        {
            // Arrange
            var car = new Car
            {
                Brand = "Dacia",
                Model = "Logan",
                Year = 2022,
                PricePerDay = 100,
            };

            // Act & Assert
            Assert.Equal(CarStatus.Disponibila, car.Status);
        }

        [Fact]  // verifica daca field-ul required de brand functioneaza
        // se creeaza o masina fara brand si verifica daca ob este invalid si daca eroarea e buna
        public void Car_MissingBrand_IsInvalid()
        {
            // Arrange
            var car = new Car
            {
                Model = "Logan",
                Year = 2022,
                PricePerDay = 100,
                VIN = "TESTVIN123",
                LicensePlate = "DJ 01 ABC"
            };

            // Act
            var validationContext = new ValidationContext(car);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(car, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Brand"));
        }

        [Theory] // verifica ca nu poti adauga o masina cu prea putine/ prea multe locuri
        [InlineData(1, false)]  // 1 loc -> Invalid (Range e 2-9)
        [InlineData(2, true)]   // 2 locuri -> Valid
        [InlineData(5, true)]   // 5 locuri -> Valid
        [InlineData(9, true)]   // 9 locuri -> Valid
        [InlineData(10, false)] // 10 locuri -> Invalid
        public void Car_SeatsRange_Validation(int seats, bool expectedToBeValid)
        {
            // Arrange
            var car = new Car
            {
                Brand = "Dacia",
                Model = "Logan",
                Year = 2022,
                PricePerDay = 100,
                VIN = "TESTVIN123",
                LicensePlate = "DJ 01 ABC",
                CategoryId = 1,
                Seats = seats
            };

            // Act
            var validationContext = new ValidationContext(car);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(car, validationContext, validationResults, true);

            // Assert
            bool hasSeatsError = validationResults.Exists(vr => vr.MemberNames.Contains("Seats"));
            
            if (expectedToBeValid)
            {
                Assert.False(hasSeatsError, $"Masina ar trebui sa fie valida cu {seats} locuri.");
            }
            else
            {
                Assert.True(hasSeatsError, $"Masina ar trebui sa dea eroare de validare pe campul Seats pentru {seats} locuri.");
            }
        }
    }
}