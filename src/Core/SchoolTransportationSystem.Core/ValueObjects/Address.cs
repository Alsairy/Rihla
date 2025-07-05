namespace Rihla.Core.ValueObjects
{
    public class Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public string Country { get; private set; }

        public Address(string street, string city, string state, string zipCode, string country = "USA")
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be empty", nameof(street));
            
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));
            
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State cannot be empty", nameof(state));
            
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code cannot be empty", nameof(zipCode));

            Street = street.Trim();
            City = city.Trim();
            State = state.Trim();
            ZipCode = zipCode.Trim();
            Country = country.Trim();
        }

        public string GetFullAddress()
        {
            return $"{Street}, {City}, {State} {ZipCode}, {Country}";
        }

        public string GetShortAddress()
        {
            return $"{City}, {State} {ZipCode}";
        }

        public override string ToString()
        {
            return GetFullAddress();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Address other) return false;
            return Street == other.Street && 
                   City == other.City && 
                   State == other.State && 
                   ZipCode == other.ZipCode && 
                   Country == other.Country;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, City, State, ZipCode, Country);
        }
    }
}

