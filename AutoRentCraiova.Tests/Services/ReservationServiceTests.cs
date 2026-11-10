using AutoRentCraiova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoRentCraiova.Tests.Services
{
    public class ReservationServiceTests
    {
        // functie helper care simuleaza logica de verificare a suprapunerii
        private bool IsCarAvailable(Car car, DateTime newReservationStart, DateTime newReservationEnd, List<Reservation> existingReservations)
        {
            var carReservations = existingReservations.Where(r => r.CarId == car.Id);

            foreach (var existing in carReservations)
            {
                if (newReservationStart < existing.EndDate && existing.StartDate < newReservationEnd)
                {
                    return false; 
                }
            }
            return true; 
        }

        [Fact] // testul 14
        public void IsCarAvailable_WhenReservationOverlaps_ReturnsFalse()
        {
            var car = new Car { Id = 1 };
            var existingReservations = new List<Reservation>
            {
                new Reservation { CarId = 1, StartDate = new DateTime(2024, 1, 10), EndDate = new DateTime(2024, 1, 15) }
            };

            bool isAvailable = IsCarAvailable(car, new DateTime(2024, 1, 12), new DateTime(2024, 1, 14), existingReservations);

            Assert.False(isAvailable);
        }

        [Fact] // testul 17: caz limita - rezervarea incepe exact cand alta se termina
        public void IsCarAvailable_WhenReservationStartsExactlyAtEndDate_ReturnsTrue()
        {
            // arrange
            var car = new Car { Id = 1 };
            var existingReservations = new List<Reservation>
            {
                // rezervare existenta pana pe data de 15
                new Reservation { CarId = 1, StartDate = new DateTime(2024, 1, 10), EndDate = new DateTime(2024, 1, 15) }
            };

            // act
            // incercam o rezervare care incepe fix pe 15, imediat dupa cealalta
            bool isAvailable = IsCarAvailable(car, new DateTime(2024, 1, 15), new DateTime(2024, 1, 20), existingReservations);

            // assert
            // ar trebui sa fie permis (depinde de logica de business dorita, dar tehnic nu e "over-booking" de zile intregi)
            Assert.True(isAvailable);
        }
    }
}