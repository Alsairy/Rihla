using Xunit;
using Rihla.Core.ValueObjects;

namespace SchoolTransportationSystem.Core.Tests.ValueObjects
{
    public class FullNameTests
    {
        [Fact]
        public void FullName_WithValidNames_CreatesSuccessfully()
        {
            var fullName = new FullName("John", "Doe", "Michael");

            Assert.Equal("John", fullName.FirstName);
            Assert.Equal("Doe", fullName.LastName);
            Assert.Equal("Michael", fullName.MiddleName);
            Assert.Equal("John Michael Doe", fullName.ToString());
        }

        [Fact]
        public void FullName_WithoutMiddleName_CreatesSuccessfully()
        {
            var fullName = new FullName("John", "Doe", null);

            Assert.Equal("John", fullName.FirstName);
            Assert.Equal("Doe", fullName.LastName);
            Assert.Null(fullName.MiddleName);
            Assert.Equal("John Doe", fullName.ToString());
        }

        [Theory]
        [InlineData("", "Doe", null)]
        [InlineData("John", "", null)]
        [InlineData(null, "Doe", null)]
        [InlineData("John", null, null)]
        public void FullName_WithInvalidNames_ThrowsArgumentException(string firstName, string lastName, string middleName)
        {
            Assert.Throws<ArgumentException>(() => new FullName(firstName, lastName, middleName));
        }
    }
}
