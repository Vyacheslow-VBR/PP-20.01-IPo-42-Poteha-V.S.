using Xunit;
using PotehaLibrary.Models;

namespace PotehaTests
{
    public class PartnerTypeTests
    {
        [Fact]
        public void PartnerType_CanSetAndGetProperties()
        {
            // Arrange
            var partnerType = new PartnerType();

            // Act
            partnerType.Id = 2;
            partnerType.Name = "ИП";

            // Assert
            Assert.Equal(2, partnerType.Id);
            Assert.Equal("ИП", partnerType.Name);
        }

        [Fact]
        public void PartnerType_Name_NotEmpty()
        {
            // Arrange
            var partnerType = new PartnerType();

            // Act
            partnerType.Name = "ООО";

            // Assert
            Assert.NotEmpty(partnerType.Name);
        }
    }
}