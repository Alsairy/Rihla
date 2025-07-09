using Xunit;
using Rihla.Core.ValueObjects;

namespace SchoolTransportationSystem.Core.Tests.ValueObjects
{
    public class AddressTests
    {
        [Fact]
        public void Address_WithValidData_CreatesSuccessfully()
        {
            var address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country");

            Assert.Equal("123 Main St", address.Street);
            Assert.Equal("Test City", address.City);
            Assert.Equal("Test State", address.State);
            Assert.Equal("12345", address.ZipCode);
            Assert.Equal("Test Country", address.Country);
        }

        [Fact]
        public void Address_ToString_ReturnsFormattedAddress()
        {
            var address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country");

            var result = address.ToString();

            Assert.Equal("123 Main St, Test City, Test State 12345, Test Country", result);
        }

        [Theory]
        [InlineData("", "City", "State", "12345", "Country")]
        [InlineData("Street", "", "State", "12345", "Country")]
        [InlineData("Street", "City", "", "12345", "Country")]
        [InlineData("Street", "City", "State", "", "Country")]
        [InlineData("Street", "City", "State", "12345", "")]
        public void Address_WithInvalidData_ThrowsArgumentException(string street, string city, string state, string zipCode, string country)
        {
            Assert.Throws<ArgumentException>(() => new Address(street, city, state, zipCode, country));
        }
    }
}
