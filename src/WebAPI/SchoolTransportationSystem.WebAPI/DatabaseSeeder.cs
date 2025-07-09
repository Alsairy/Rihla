using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Core.ValueObjects;
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
                    Console.WriteLine("Database already contains data. Clearing for large dataset generation...");
                    
                    context.Attendances.RemoveRange(context.Attendances);
                    context.Payments.RemoveRange(context.Payments);
                    context.MaintenanceRecords.RemoveRange(context.MaintenanceRecords);
                    context.Trips.RemoveRange(context.Trips);
                    context.Students.RemoveRange(context.Students);
                    context.Drivers.RemoveRange(context.Drivers);
                    context.Vehicles.RemoveRange(context.Vehicles);
                    context.Routes.RemoveRange(context.Routes);
                    context.Users.RemoveRange(context.Users);
                    context.Notifications.RemoveRange(context.Notifications);
                    
                    await context.SaveChangesAsync();
                    Console.WriteLine("Existing data cleared. Generating large dataset...");
                }

                var users = new List<User>();
                var random = new Random(42); // Fixed seed for reproducible data
                var (passwordHash, passwordSalt) = HashPassword("password123");
                
                var personaTestUsers = new List<User>
                {
                    new User
                    {
                        TenantId = "1",
                        Username = "superadmin",
                        Email = "superadmin@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "SuperAdmin",
                        FirstName = "Super",
                        LastName = "Administrator",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "admin",
                        Email = "admin@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Admin",
                        FirstName = "Tenant",
                        LastName = "Administrator",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "sysadmin",
                        Email = "sysadmin@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "SystemAdmin",
                        FirstName = "System",
                        LastName = "Administrator",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "dispatcher",
                        Email = "dispatcher@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Dispatcher",
                        FirstName = "Main",
                        LastName = "Dispatcher",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        TenantId = "1",
                        Username = "maintenance",
                        Email = "maintenance@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Maintenance",
                        FirstName = "Fleet",
                        LastName = "Maintenance",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                users.AddRange(personaTestUsers);
                
                var driverFirstNames = new[] { "Ahmed", "Mohammed", "Abdullah", "Omar", "Khalid", "Fahad", "Saud", "Faisal", "Nasser", "Turki", "Bandar", "Majed", "Rayan", "Ziad", "Waleed" };
                var driverLastNames = new[] { "Al-Rashid", "Al-Otaibi", "Al-Dosari", "Al-Harbi", "Al-Ghamdi", "Al-Zahrani", "Al-Maliki", "Al-Shehri", "Al-Qahtani", "Al-Mutairi" };
                
                for (int i = 1; i <= 500; i++)
                {
                    var firstName = driverFirstNames[random.Next(driverFirstNames.Length)];
                    var lastName = driverLastNames[random.Next(driverLastNames.Length)];
                    
                    users.Add(new User
                    {
                        TenantId = "1",
                        Username = $"driver{i:D3}",
                        Email = $"driver{i:D3}@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Driver",
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = random.Next(100) < 95, // 95% active
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }
                
                var parentFirstNames = new[] { "Fatima", "Aisha", "Maryam", "Khadija", "Zainab", "Sarah", "Nora", "Lina", "Reem", "Hala", "Ahmed", "Mohammed", "Abdullah", "Omar", "Khalid" };
                var parentLastNames = new[] { "Al-Rashid", "Al-Otaibi", "Al-Dosari", "Al-Harbi", "Al-Ghamdi", "Al-Zahrani", "Al-Maliki", "Al-Shehri", "Al-Qahtani", "Al-Mutairi" };
                
                for (int i = 1; i <= 2000; i++)
                {
                    var firstName = parentFirstNames[random.Next(parentFirstNames.Length)];
                    var lastName = parentLastNames[random.Next(parentLastNames.Length)];
                    
                    users.Add(new User
                    {
                        TenantId = "1",
                        Username = $"parent{i:D4}",
                        Email = $"parent{i:D4}@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Parent",
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = random.Next(100) < 98, // 98% active
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 730))
                    });
                }
                
                var studentFirstNames = new[] { "Ali", "Hassan", "Hussein", "Yusuf", "Ibrahim", "Ismail", "Yahya", "Zakaria", "Adam", "Noah", "Layla", "Amina", "Yasmin", "Salma", "Jana" };
                var studentLastNames = new[] { "Al-Rashid", "Al-Otaibi", "Al-Dosari", "Al-Harbi", "Al-Ghamdi", "Al-Zahrani", "Al-Maliki", "Al-Shehri", "Al-Qahtani", "Al-Mutairi" };
                
                for (int i = 1; i <= 3000; i++)
                {
                    var firstName = studentFirstNames[random.Next(studentFirstNames.Length)];
                    var lastName = studentLastNames[random.Next(studentLastNames.Length)];
                    
                    users.Add(new User
                    {
                        TenantId = "1",
                        Username = $"student{i:D4}",
                        Email = $"student{i:D4}@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Student",
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = random.Next(100) < 97, // 97% active
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 1095))
                    });
                }
                
                for (int i = 1; i <= 50; i++)
                {
                    var firstName = driverFirstNames[random.Next(driverFirstNames.Length)];
                    var lastName = driverLastNames[random.Next(driverLastNames.Length)];
                    
                    users.Add(new User
                    {
                        TenantId = "1",
                        Username = $"dispatch{i:D2}",
                        Email = $"dispatch{i:D2}@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Dispatcher",
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = random.Next(100) < 96, // 96% active
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }
                
                for (int i = 1; i <= 25; i++)
                {
                    var firstName = driverFirstNames[random.Next(driverFirstNames.Length)];
                    var lastName = driverLastNames[random.Next(driverLastNames.Length)];
                    
                    users.Add(new User
                    {
                        TenantId = "1",
                        Username = $"maint{i:D2}",
                        Email = $"maint{i:D2}@rihla.com",
                        PasswordHash = passwordHash,
                        Salt = passwordSalt,
                        Role = "Maintenance",
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = random.Next(100) < 94, // 94% active
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }

                context.Users.AddRange(users);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {users.Count} users across all personas");

                var drivers = new List<Driver>();
                var driverUsers = users.Where(u => u.Role == "Driver").ToList();
                var cities = new[] { "Riyadh", "Jeddah", "Dammam", "Mecca", "Medina", "Khobar", "Tabuk", "Abha", "Buraidah", "Khamis Mushait" };
                var districts = new[] { "Al-Malaz", "Al-Olaya", "Al-Sulaimaniyah", "King Fahd", "Al-Nakheel", "Al-Rawdah", "Al-Aziziyah", "Al-Hamra", "Al-Shifa", "Al-Naseem" };
                
                for (int i = 0; i < driverUsers.Count; i++)
                {
                    var driverUser = driverUsers[i];
                    var city = cities[random.Next(cities.Length)];
                    var district = districts[random.Next(districts.Length)];
                    
                    drivers.Add(new Driver
                    {
                        TenantId = 1,
                        EmployeeNumber = $"DRV{(i + 1):D4}",
                        FullName = new FullName(driverUser.FirstName, driverUser.LastName, "Abdullah"),
                        LicenseNumber = $"LIC{(i + 1):D6}",
                        LicenseExpiry = DateTime.UtcNow.AddYears(random.Next(1, 5)),
                        Phone = $"+96650{1000000 + i:D7}",
                        Email = driverUser.Email,
                        Address = new Address($"{random.Next(1, 999)} {district} St", city, $"{city} Province", $"{random.Next(10000, 99999)}", "Saudi Arabia"),
                        HireDate = DateTime.UtcNow.AddDays(-random.Next(30, 1825)), // Hired within last 5 years
                        DateOfBirth = DateTime.UtcNow.AddYears(-random.Next(25, 55)), // Age 25-55
                        Status = driverUser.IsActive ? DriverStatus.Active : (DriverStatus)random.Next(2, 5),
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }

                context.Drivers.AddRange(drivers);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {drivers.Count} drivers");

                var vehicles = new List<Vehicle>();
                var vehicleMakes = new[] { "Mercedes-Benz", "Toyota", "Hyundai", "Isuzu", "Mitsubishi", "Ford", "Nissan" };
                var busModels = new[] { "Sprinter", "Coaster", "Rosa", "County", "Fuso", "Transit", "Urvan" };
                var colors = new[] { "Yellow", "White", "Blue", "Orange", "Green", "Red" };
                var fuelTypes = new[] { "Diesel", "Gasoline", "Hybrid" };
                
                for (int i = 1; i <= 300; i++)
                {
                    var make = vehicleMakes[random.Next(vehicleMakes.Length)];
                    var model = busModels[random.Next(busModels.Length)];
                    var year = 2018 + random.Next(0, 7); // 2018-2024
                    var capacity = new[] { 14, 20, 25, 30, 35, 45, 50 }[random.Next(7)];
                    var color = colors[random.Next(colors.Length)];
                    var fuelType = fuelTypes[random.Next(fuelTypes.Length)];
                    var status = (VehicleStatus)random.Next(1, 5);
                    var type = capacity <= 14 ? VehicleType.MiniBus : capacity <= 25 ? VehicleType.Van : VehicleType.Bus;
                    
                    vehicles.Add(new Vehicle
                    {
                        TenantId = 1,
                        VehicleNumber = $"BUS{i:D3}",
                        Make = make,
                        Model = model,
                        Year = year,
                        Color = color,
                        Type = type,
                        Capacity = capacity,
                        LicensePlate = $"{(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}-{random.Next(1000, 9999)}",
                        VIN = $"VIN{random.Next(100000000, 999999999)}{(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}",
                        Status = status,
                        Mileage = random.Next(10000, 300000), // 10k-300k km
                        FuelType = fuelType,
                        PurchaseDate = DateTime.UtcNow.AddDays(-random.Next(365, 2555)), // Purchased within last 7 years
                        PurchasePrice = 80000 + random.Next(0, 200000), // 80k-280k SAR
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }

                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {vehicles.Count} vehicles");

                var routes = new List<SchoolTransportationSystem.Core.Entities.Route>();
                var routeNames = new[] { "North District", "South District", "East District", "West District", "Central Area", "Industrial Area", "Residential Complex", "Villa Compound", "Downtown", "Suburbs" };
                var schools = new[] { "Al-Noor International School", "King Fahd Academy", "Riyadh International School", "Al-Faisal University", "Princess Nourah School", "Al-Manahil School", "Dar Al-Fikr School", "Al-Rowad School" };
                
                for (int i = 1; i <= 150; i++)
                {
                    var routeName = routeNames[random.Next(routeNames.Length)];
                    var school = schools[random.Next(schools.Length)];
                    var distance = 5.0m + (decimal)(random.NextDouble() * 25.0); // 5-30 km
                    var duration = (int)(distance * 2.5m + random.Next(-10, 20)); // Roughly 2.5 min per km + variation
                    var startHour = 6 + random.Next(0, 3); // Start between 6-8 AM
                    var startMinute = random.Next(0, 4) * 15; // 0, 15, 30, 45 minutes
                    
                    routes.Add(new SchoolTransportationSystem.Core.Entities.Route
                    {
                        TenantId = 1,
                        RouteNumber = $"RT{i:D3}",
                        Name = $"{(i % 2 == 0 ? "Morning" : "Afternoon")} Route - {routeName}",
                        Description = $"Covers {routeName.ToLower()} serving {school}",
                        Status = random.Next(100) < 90 ? RouteStatus.Active : (RouteStatus)random.Next(2, 5), // 90% active
                        StartTime = new TimeSpan(startHour, startMinute, 0),
                        EndTime = new TimeSpan(startHour + 1, startMinute + Math.Max(15, duration), 0),
                        Distance = Math.Round(distance, 1),
                        EstimatedDuration = Math.Max(15, duration), // Minimum 15 minutes
                        StartLocation = $"{routeName} Terminal",
                        EndLocation = $"{school} Main Gate",
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }

                context.Routes.AddRange(routes);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {routes.Count} routes");

                var students = new List<Student>();
                var studentUsers = users.Where(u => u.Role == "Student").ToList();
                var parentUsers = users.Where(u => u.Role == "Parent").ToList();
                var grades = new[] { "KG1", "KG2", "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8", "Grade 9", "Grade 10", "Grade 11", "Grade 12" };
                
                for (int i = 0; i < studentUsers.Count; i++)
                {
                    var studentUser = studentUsers[i];
                    var parentUser = parentUsers[i % parentUsers.Count]; // Assign parents cyclically
                    var grade = grades[random.Next(grades.Length)];
                    var school = schools[random.Next(schools.Length)];
                    var route = routes[random.Next(routes.Count)];
                    var city = cities[random.Next(cities.Length)];
                    var district = districts[random.Next(districts.Length)];
                    var age = grade.Contains("KG") ? random.Next(4, 6) : 
                             grade.Contains("1") || grade.Contains("2") || grade.Contains("3") ? random.Next(6, 9) :
                             grade.Contains("4") || grade.Contains("5") || grade.Contains("6") ? random.Next(9, 12) :
                             grade.Contains("7") || grade.Contains("8") || grade.Contains("9") ? random.Next(12, 15) :
                             random.Next(15, 18);
                    
                    students.Add(new Student
                    {
                        TenantId = 1,
                        StudentNumber = $"STU{(i + 1):D4}",
                        FullName = new FullName(studentUser.FirstName, studentUser.LastName, "Abdullah"),
                        DateOfBirth = DateTime.UtcNow.AddYears(-age),
                        Grade = grade,
                        School = school,
                        Address = new Address($"{random.Next(100, 999)} {district} District", city, $"{city} Province", $"{random.Next(10000, 99999)}", "Saudi Arabia"),
                        Phone = $"+96656{3000000 + i:D7}",
                        Email = studentUser.Email,
                        ParentName = $"{parentUser.FirstName} {parentUser.LastName}",
                        ParentPhone = $"+96655{2000000 + (i % parentUsers.Count):D7}",
                        Status = random.Next(100) < 95 ? StudentStatus.Active : (StudentStatus)random.Next(2, 5), // 95% active
                        RouteId = route.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    });
                }

                context.Students.AddRange(students);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {students.Count} students");

                var trips = new List<Trip>();
                var tripStatuses = new[] { TripStatus.Scheduled, TripStatus.InProgress, TripStatus.Completed, TripStatus.Cancelled, TripStatus.Delayed };
                
                for (int i = 0; i < 1000; i++)
                {
                    var route = routes[random.Next(routes.Count)];
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var driver = drivers[random.Next(drivers.Count)];
                    var daysOffset = random.Next(-30, 30); // Past 30 days to future 30 days
                    var baseDate = DateTime.UtcNow.Date.AddDays(daysOffset);
                    var startHour = 6 + random.Next(0, 12); // 6 AM to 6 PM
                    var duration = random.Next(30, 120); // 30 minutes to 2 hours
                    
                    var status = daysOffset < -1 ? TripStatus.Completed : // Past trips are completed
                                daysOffset > 1 ? TripStatus.Scheduled : // Future trips are scheduled
                                tripStatuses[random.Next(tripStatuses.Length)]; // Current trips have mixed status
                    
                    trips.Add(new Trip
                    {
                        TenantId = 1,
                        RouteId = route.Id,
                        VehicleId = vehicle.Id,
                        DriverId = driver.Id,
                        ScheduledStartTime = baseDate.AddHours(startHour),
                        ScheduledEndTime = baseDate.AddHours(startHour).AddMinutes(duration),
                        ActualStartTime = status == TripStatus.Completed || status == TripStatus.InProgress ? 
                                        baseDate.AddHours(startHour).AddMinutes(random.Next(-10, 10)) : null,
                        ActualEndTime = status == TripStatus.Completed ? 
                                      baseDate.AddHours(startHour).AddMinutes(duration + random.Next(-15, 15)) : null,
                        Status = status,
                        Notes = random.Next(100) < 20 ? "Traffic delay reported" : null, // 20% have notes
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                    });
                }

                context.Trips.AddRange(trips);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {trips.Count} trips");

                var payments = new List<Payment>();
                var paymentTypes = new[] { PaymentType.OneTime, PaymentType.Recurring };
                var paymentStatuses = new[] { PaymentStatus.Pending, PaymentStatus.Processing, PaymentStatus.Completed, PaymentStatus.Cancelled };
                var paymentMethods = new[] { "Credit Card", "Bank Transfer", "Cash", "Check", "Online Payment" };
                
                for (int i = 0; i < 5000; i++)
                {
                    var student = students[random.Next(students.Count)];
                    var paymentType = paymentTypes[random.Next(paymentTypes.Length)];
                    var amount = paymentType == PaymentType.Recurring ? 
                               300 + random.Next(0, 500) : // Monthly fees 300-800 SAR
                               50 + random.Next(0, 200);   // One-time fees 50-250 SAR
                    var daysOffset = random.Next(-90, 30); // Past 90 days to future 30 days
                    var dueDate = DateTime.UtcNow.AddDays(daysOffset);
                    var status = daysOffset < -30 ? PaymentStatus.Completed : // Old payments are completed
                                daysOffset < 0 ? (random.Next(100) < 80 ? PaymentStatus.Completed : PaymentStatus.Processing) : // Recent payments mostly completed
                                PaymentStatus.Pending; // Future payments are pending
                    
                    payments.Add(new Payment
                    {
                        TenantId = 1,
                        StudentId = student.Id,
                        PaymentNumber = $"PAY{(i + 1):D5}",
                        Amount = amount,
                        Currency = "SAR",
                        Type = paymentType,
                        Status = status,
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                        DueDate = dueDate,
                        PaidDate = status == PaymentStatus.Completed ? dueDate.AddDays(random.Next(-5, 5)) : null,
                        Description = paymentType == PaymentType.Recurring ? 
                                    $"Monthly transportation fee - {dueDate:MMMM yyyy}" : 
                                    "One-time transportation service",
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 120))
                    });
                }

                context.Payments.AddRange(payments);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {payments.Count} payments");

                var maintenanceRecords = new List<MaintenanceRecord>();
                var maintenanceTypes = new[] { "Preventive", "Corrective", "Emergency", "Inspection", "Repair" };
                var maintenanceDescriptions = new[] { 
                    "Regular oil change and filter replacement",
                    "Brake system inspection and repair",
                    "Engine diagnostic and tune-up",
                    "Tire replacement and alignment",
                    "Air conditioning system service",
                    "Transmission fluid change",
                    "Battery replacement",
                    "Suspension system repair",
                    "Electrical system diagnosis",
                    "Annual safety inspection"
                };
                
                for (int i = 0; i < 800; i++)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var maintenanceType = maintenanceTypes[random.Next(maintenanceTypes.Length)];
                    var description = maintenanceDescriptions[random.Next(maintenanceDescriptions.Length)];
                    var daysOffset = random.Next(-180, 60); // Past 6 months to future 2 months
                    var scheduledDate = DateTime.UtcNow.AddDays(daysOffset);
                    var isCompleted = daysOffset < 0; // Past maintenance is completed
                    var cost = maintenanceType == "Emergency" ? 1000 + random.Next(0, 3000) : // Emergency 1000-4000 SAR
                              maintenanceType == "Preventive" ? 200 + random.Next(0, 800) : // Preventive 200-1000 SAR
                              500 + random.Next(0, 1500); // Others 500-2000 SAR
                    
                    maintenanceRecords.Add(new MaintenanceRecord
                    {
                        TenantId = 1,
                        VehicleId = vehicle.Id,
                        MaintenanceType = maintenanceType,
                        Description = description,
                        ScheduledDate = scheduledDate,
                        CompletedDate = isCompleted ? scheduledDate.AddDays(random.Next(0, 3)) : null,
                        Cost = cost,
                        Currency = "SAR",
                        IsCompleted = isCompleted,
                        ServiceProvider = random.Next(100) < 70 ? "Al-Rashid Auto Service" : "Quick Fix Garage", // 70% use main provider
                        Notes = random.Next(100) < 30 ? "Additional parts required" : null, // 30% have notes
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 200))
                    });
                }

                context.MaintenanceRecords.AddRange(maintenanceRecords);
                await context.SaveChangesAsync();
                Console.WriteLine($"Generated {maintenanceRecords.Count} maintenance records");

                Console.WriteLine("\n🎉 Large dataset generation completed successfully!");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"📊 COMPREHENSIVE DATA SUMMARY:");
                Console.WriteLine($"   👥 Users: {users.Count} (across all 8 personas)");
                Console.WriteLine($"   🚗 Drivers: {drivers.Count}");
                Console.WriteLine($"   🚌 Vehicles: {vehicles.Count}");
                Console.WriteLine($"   🗺️  Routes: {routes.Count}");
                Console.WriteLine($"   🎓 Students: {students.Count}");
                Console.WriteLine($"   🚌 Trips: {trips.Count}");
                Console.WriteLine($"   💰 Payments: {payments.Count}");
                Console.WriteLine($"   🔧 Maintenance Records: {maintenanceRecords.Count}");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"📈 TOTAL RECORDS: {users.Count + drivers.Count + vehicles.Count + routes.Count + students.Count + trips.Count + payments.Count + maintenanceRecords.Count}");
                Console.WriteLine("🚀 System ready for comprehensive testing with realistic data volumes!");
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
