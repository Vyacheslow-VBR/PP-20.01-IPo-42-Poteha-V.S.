using Xunit;
using PotehaLibrary.Models;

namespace PotehaTests
{
    public class ProductTests
    {
        [Fact]
        public void Product_CanSetAndGetProperties()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 5;
            product.Name = "Тестовый продукт";
            product.Article = "ART-123";
            product.Price = 999.99m;

            // Assert
            Assert.Equal(5, product.Id);
            Assert.Equal("Тестовый продукт", product.Name);
            Assert.Equal("ART-123", product.Article);
            Assert.Equal(999.99m, product.Price);
        }

        [Fact]
        public void Product_Price_CanBeZero()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Price = 0m;

            // Assert
            Assert.Equal(0m, product.Price);
        }
    }
}