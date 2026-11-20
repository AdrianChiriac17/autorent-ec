using AutoRentCraiova.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AutoRentCraiova.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact] // testul 12: verifica validarea pentru email in viewmodel-ul de login
        public void LoginViewModel_InvalidEmail_IsInvalid()
        {
            // arrange
            var viewModel = new LoginViewModel
            {
                Email = "email-invalid", // nu are @ si domeniu
                Password = "password123" // parola valida
            };

            // act
            var validationContext = new ValidationContext(viewModel);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(viewModel, validationContext, validationResults, true);

            // assert
            Assert.False(isValid);
            // verificam ca eroarea este pe campul email
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Email"));
        }
    }
}