using Xunit;
using PotehaLibrary.Models;
using System;

namespace PotehaTests
{
    public class SaleTests
    {
        [Fact]
        public void Sale_CanSetAndGetProperties()
        {
            // Arrange
            var sale = new Sale();
            var testDate = DateTime.Now;

            // Act
            sale.Id = 15;
            sale.PartnerId = 3;
            sale.ProductId = 7;
            sale.Quantity = 25;
            sale.TotalAmount = 5000m;
            sale.SaleDate = testDate;

            // Assert
            Assert.Equal(15, sale.Id);
            Assert.Equal(3, sale.PartnerId);
            Assert.Equal(7, sale.ProductId);
            Assert.Equal(25, sale.Quantity);
            Assert.Equal(5000m, sale.TotalAmount);
            Assert.Equal(testDate, sale.SaleDate);
        }

        [Fact]
        public void Sale_Quantity_Positive()
        {
            // Arrange
            var sale = new Sale();

            // Act
            sale.Quantity = 1;

            // Assert
            Assert.True(sale.Quantity > 0);
        }
    }
}