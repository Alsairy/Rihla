using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Core.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace SchoolTransportationSystem.WebAPI
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                await context.Database.EnsureCreatedAsync();
                
                if (context.Students.Any() || context.Drivers.Any() || context.Vehicles.Any() || context.Routes.Any() || context.Users.Any())
                {
                    return; // Database already seeded
                }

                var (adminHash, adminSalt) = HashPassword("password123");
                var (parentHash, parentSalt) = HashPassword("password123");
                var (driverHash, driverSalt) = HashPassword("password123");

                var users = new List<User>
                {
                    new User
                    {
                        TenantId = "1",
                        Username = "admin",
                        Email = "admin@rihla.com",
                        PasswordHash = adminHash,
                        Salt = adminSalt,
                        Role = "Admin",
                        FirstName = "System",
                        LastName = "Administrator",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "parent1",
                        Email = "parent@rihla.com",
                        PasswordHash = parentHash,
                        Salt = parentSalt,
                        Role = "Parent",
                        FirstName = "Mohammed",
                        LastName = "Al-Ahmad",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "driver1",
                        Email = "driver@rihla.com",
                        PasswordHash = driverHash,
                        Salt = driverSalt,
                        Role = "Driver",
                        FirstName = "Ahmed",
                        LastName = "Al-Rashid",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                var drivers = new List<Driver>
                {
                    new Driver
                    {
                        TenantId = 1,
                        EmployeeNumber = "DRV001",
                        FullName = new FullName("Ahmed", "Al-Rashid", "Mohammed"),
                        LicenseNumber = "LIC001234",
                        LicenseExpiry = DateTime.UtcNow.AddYears(2),
                        Phone = "+966501234567",
                        Email = "ahmed.rashid@rihla.com",
                        Address = new Address("123 King Fahd Road", "Riyadh", "Riyadh Province", "12345", "Saudi Arabia"),
                        HireDate = DateTime.UtcNow.AddYears(-2),
                        DateOfBirth = DateTime.UtcNow.AddYears(-35),
                        Status = DriverStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Driver
                    {
                        TenantId = 1,
                        EmployeeNumber = "DRV002",
                        FullName = new FullName("Omar", "Al-Zahra", "Abdullah"),
                        LicenseNumber = "LIC005678",
                        LicenseExpiry = DateTime.UtcNow.AddYears(3),
                        Phone = "+966502345678",
                        Email = "omar.zahra@rihla.com",
                        Address = new Address("456 Prince Sultan Road", "Jeddah", "Makkah Province", "23456", "Saudi Arabia"),
                        HireDate = DateTime.UtcNow.AddYears(-1),
                        DateOfBirth = DateTime.UtcNow.AddYears(-28),
                        Status = DriverStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Drivers.AddRange(drivers);
                await context.SaveChangesAsync();

                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        TenantId = 1,
                        VehicleNumber = "BUS001",
                        Make = "Mercedes-Benz",
                        Model = "Sprinter",
                        Year = 2022,
                        Color = "Yellow",
                        Type = VehicleType.Bus,
                        Capacity = 25,
                        LicensePlate = "ABC-1234",
                        VIN = "WDB9066351234567",
                        Status = VehicleStatus.Active,
                        Mileage = 15000,
                        FuelType = "Diesel",
                        PurchaseDate = DateTime.UtcNow.AddYears(-2),
                        PurchasePrice = 150000,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Vehicle
                    {
                        TenantId = 1,
                        VehicleNumber = "BUS002",
                        Make = "Toyota",
                        Model = "Coaster",
                        Year = 2021,
                        Color = "White",
                        Type = VehicleType.Bus,
                        Capacity = 30,
                        LicensePlate = "DEF-5678",
                        VIN = "JTFDY3H50B1234567",
                        Status = VehicleStatus.Active,
                        Mileage = 22000,
                        FuelType = "Diesel",
                        PurchaseDate = DateTime.UtcNow.AddYears(-1),
                        PurchasePrice = 120000,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();

                var routes = new List<Rihla.Core.Entities.Route>
                {
                    new Rihla.Core.Entities.Route
                    {
                        TenantId = 1,
                        RouteNumber = "RT001",
                        Name = "Morning Route - Riyadh North",
                        Description = "Primary morning route covering northern Riyadh districts",
                        Status = RouteStatus.Active,
                        StartTime = new TimeSpan(7, 0, 0),
                        EndTime = new TimeSpan(8, 30, 0),
                        Distance = 25.5m,
                        EstimatedDuration = 90,
                        StartLocation = "Al-Nakheel District",
                        EndLocation = "Al-Noor Elementary School",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Rihla.Core.Entities.Route
                    {
                        TenantId = 1,
                        RouteNumber = "RT002",
                        Name = "Morning Route - Riyadh South", 
                        Description = "Secondary morning route covering southern Riyadh districts",
                        Status = RouteStatus.Active,
                        StartTime = new TimeSpan(7, 15, 0),
                        EndTime = new TimeSpan(8, 45, 0),
                        Distance = 18.2m,
                        EstimatedDuration = 90,
                        StartLocation = "Al-Rawda District",
                        EndLocation = "Al-Noor Elementary School",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Routes.AddRange(routes);
                await context.SaveChangesAsync();

                var students = new List<Student>
                {
                    new Student
                    {
                        TenantId = 1,
                        StudentNumber = "STU001",
                        FullName = new FullName("Sara", "Al-Ahmad", "Mohammed"),
                        DateOfBirth = DateTime.UtcNow.AddYears(-8),
                        Grade = "Grade 3",
                        School = "Al-Noor Elementary School",
                        Address = new Address("100 Al-Malaz District", "Riyadh", "Riyadh Province", "11564", "Saudi Arabia"),
                        Phone = "+966501111111",
                        Email = "sara.ahmad@email.com",
                        ParentName = "Mohammed Al-Ahmad",
                        ParentPhone = "+966501111111",
                        Status = StudentStatus.Active,
                        RouteId = routes[0].Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        TenantId = 1,
                        StudentNumber = "STU002",
                        FullName = new FullName("Ali", "Al-Otaibi", "Abdullah"),
                        DateOfBirth = DateTime.UtcNow.AddYears(-10),
                        Grade = "Grade 5",
                        School = "Al-Noor Elementary School",
                        Address = new Address("200 Al-Naseem District", "Riyadh", "Riyadh Province", "11564", "Saudi Arabia"),
                        Phone = "+966503333333",
                        Email = "ali.otaibi@email.com",
                        ParentName = "Abdullah Al-Otaibi",
                        ParentPhone = "+966503333333",
                        Status = StudentStatus.Active,
                        RouteId = routes[0].Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Student
                    {
                        TenantId = 1,
                        StudentNumber = "STU003",
                        FullName = new FullName("Fatima", "Al-Harbi", "Salem"),
                        DateOfBirth = DateTime.UtcNow.AddYears(-7),
                        Grade = "Grade 2",
                        School = "Al-Noor Elementary School",
                        Address = new Address("300 Al-Rawda District", "Riyadh", "Riyadh Province", "11564", "Saudi Arabia"),
                        Phone = "+966505555555",
                        Email = "fatima.harbi@email.com",
                        ParentName = "Salem Al-Harbi",
                        ParentPhone = "+966505555555",
                        Status = StudentStatus.Active,
                        RouteId = routes[1].Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Students.AddRange(students);
                await context.SaveChangesAsync();

                var trips = new List<Trip>
                {
                    new Trip
                    {
                        TenantId = 1,
                        RouteId = routes[0].Id,
                        VehicleId = vehicles[0].Id,
                        DriverId = drivers[0].Id,
                        ScheduledStartTime = DateTime.UtcNow.Date.AddHours(7),
                        ScheduledEndTime = DateTime.UtcNow.Date.AddHours(8),
                        Status = TripStatus.Scheduled,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Trips.AddRange(trips);
                await context.SaveChangesAsync();

                var payments = new List<Payment>
                {
                    new Payment
                    {
                        TenantId = 1,
                        StudentId = students[0].Id,
                        PaymentNumber = "PAY001",
                        Amount = 500.00m,
                        Currency = "SAR",
                        Type = PaymentType.Recurring,
                        Status = PaymentStatus.Pending,
                        DueDate = DateTime.UtcNow.AddDays(15),
                        Description = "Monthly transportation fee",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Payments.AddRange(payments);
                await context.SaveChangesAsync();

                var maintenanceRecords = new List<MaintenanceRecord>
                {
                    new MaintenanceRecord
                    {
                        TenantId = 1,
                        VehicleId = vehicles[0].Id,
                        MaintenanceType = "Preventive",
                        Description = "Regular service",
                        ScheduledDate = DateTime.UtcNow.AddDays(7),
                        Cost = 800.00m,
                        Currency = "SAR",
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.MaintenanceRecords.AddRange(maintenanceRecords);
                await context.SaveChangesAsync();

                Console.WriteLine("Database seeded successfully!");
                Console.WriteLine($"Added: {users.Count} users, {drivers.Count} drivers, {vehicles.Count} vehicles, {routes.Count} routes, {students.Count} students");
                Console.WriteLine($"Added: {trips.Count} trips, {payments.Count} payments, {maintenanceRecords.Count} maintenance records");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        private static (string hash, string salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return (hash, salt);
        }
    }
}
