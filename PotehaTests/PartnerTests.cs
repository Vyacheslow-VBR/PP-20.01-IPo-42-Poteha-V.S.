using Xunit;
using PotehaLibrary.Models;
using System.Collections.Generic;

namespace PotehaTests
{
    public class PartnerTests
    {
        [Fact]
        public void Partner_DefaultDiscount_IsZero()
        {
            // Arrange
            var partner = new Partner();

            // Act
            var discount = partner.Discount;

            // Assert
            Assert.Equal(0, discount);
        }

        [Fact]
        public void Partner_CanSetAndGetProperties()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Id = 10;
            partner.Name = "Тестовый партнер";
            partner.Rating = 5;
            partner.Discount = 10;

            // Assert
            Assert.Equal(10, partner.Id);
            Assert.Equal("Тестовый партнер", partner.Name);
            Assert.Equal(5, partner.Rating);
            Assert.Equal(10, partner.Discount);
        }

        [Fact]
        public void Partner_Director_CanBeNull()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.DirectorFullname = null;

            // Assert
            Assert.Null(partner.DirectorFullname);
        }

        [Fact]
        public void Partner_Phone_CanBeNull()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Phone = null;

            // Assert
            Assert.Null(partner.Phone);
        }
    }
}