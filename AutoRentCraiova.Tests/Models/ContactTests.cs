using AutoRentCraiova.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AutoRentCraiova.Tests.Models
{
    public class ContactTests
    {
        [Fact] // testul 13: verifica validarea pentru modelul de contact
        public void Contact_MissingRequiredFields_IsInvalid()
        {
            // arrange
            var contact = new Contact(); // cream un obiect gol, fara nicio proprietate setata

            // act
            var validationContext = new ValidationContext(contact);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(contact, validationContext, validationResults, true);

            // assert
            Assert.False(isValid);
            // verificam ca avem erori pe cele 3 campuri obligatorii
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Name"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Email"));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Message"));
        }
    }
}