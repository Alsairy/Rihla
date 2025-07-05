namespace Rihla.Core.ValueObjects
{
    public class FullName
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? MiddleName { get; private set; }

        public FullName(string firstName, string lastName, string? middleName = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
        }

        public string GetFullName()
        {
            return string.IsNullOrEmpty(MiddleName) 
                ? $"{FirstName} {LastName}"
                : $"{FirstName} {MiddleName} {LastName}";
        }

        public string GetDisplayName()
        {
            return $"{FirstName} {LastName}";
        }

        public override string ToString()
        {
            return GetFullName();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not FullName other) return false;
            return FirstName == other.FirstName && 
                   LastName == other.LastName && 
                   MiddleName == other.MiddleName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, MiddleName);
        }
    }
}

