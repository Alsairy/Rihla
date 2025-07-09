using Microsoft.EntityFrameworkCore;
using Rihla.Core.Entities;
using Rihla.Core.ValueObjects;

namespace Rihla.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Student> Students { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteStop> RouteStops { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure concurrency control for all entities
            // ConfigureConcurrencyControl(modelBuilder);

            // Configure value objects
            ConfigureValueObjects(modelBuilder);
            
            // Configure entities
            ConfigureStudent(modelBuilder);
            ConfigureDriver(modelBuilder);
            ConfigureVehicle(modelBuilder);
            ConfigureRoute(modelBuilder);
            ConfigureRouteStop(modelBuilder);
            ConfigureTrip(modelBuilder);
            ConfigureAttendance(modelBuilder);
            ConfigurePayment(modelBuilder);
            ConfigureMaintenanceRecord(modelBuilder);
            ConfigureUser(modelBuilder);
            ConfigureNotification(modelBuilder);

            // Configure relationships
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureValueObjects(ModelBuilder modelBuilder)
        {
            // Configure FullName value object
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.FullName, fn =>
                {
                    fn.Property(f => f.FirstName).HasMaxLength(50).IsRequired();
                    fn.Property(f => f.LastName).HasMaxLength(50).IsRequired();
                    fn.Property(f => f.MiddleName).HasMaxLength(50);
                });

            modelBuilder.Entity<Driver>()
                .OwnsOne(d => d.FullName, fn =>
                {
                    fn.Property(f => f.FirstName).HasMaxLength(50).IsRequired();
                    fn.Property(f => f.LastName).HasMaxLength(50).IsRequired();
                    fn.Property(f => f.MiddleName).HasMaxLength(50);
                });

            // Configure Address value object
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.Address, a =>
                {
                    a.Property(ad => ad.Street).HasMaxLength(200).IsRequired();
                    a.Property(ad => ad.City).HasMaxLength(100).IsRequired();
                    a.Property(ad => ad.State).HasMaxLength(50).IsRequired();
                    a.Property(ad => ad.ZipCode).HasMaxLength(20).IsRequired();
                    a.Property(ad => ad.Country).HasMaxLength(50).IsRequired();
                });

            modelBuilder.Entity<Driver>()
                .OwnsOne(d => d.Address, a =>
                {
                    a.Property(ad => ad.Street).HasMaxLength(200).IsRequired();
                    a.Property(ad => ad.City).HasMaxLength(100).IsRequired();
                    a.Property(ad => ad.State).HasMaxLength(50).IsRequired();
                    a.Property(ad => ad.ZipCode).HasMaxLength(20).IsRequired();
                    a.Property(ad => ad.Country).HasMaxLength(50).IsRequired();
                });

            modelBuilder.Entity<RouteStop>()
                .OwnsOne(rs => rs.Address, a =>
                {
                    a.Property(ad => ad.Street).HasMaxLength(200).IsRequired();
                    a.Property(ad => ad.City).HasMaxLength(100).IsRequired();
                    a.Property(ad => ad.State).HasMaxLength(50).IsRequired();
                    a.Property(ad => ad.ZipCode).HasMaxLength(20).IsRequired();
                    a.Property(ad => ad.Country).HasMaxLength(50).IsRequired();
                });
        }

        private void ConfigureStudent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Grade).HasMaxLength(20).IsRequired();
                entity.Property(e => e.School).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.ParentName).HasMaxLength(100);
                entity.Property(e => e.ParentPhone).HasMaxLength(20);
                entity.Property(e => e.ParentEmail).HasMaxLength(100);
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);
                entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
                entity.Property(e => e.SpecialNeeds).HasMaxLength(500);
                entity.Property(e => e.MedicalConditions).HasMaxLength(500);
                entity.Property(e => e.Allergies).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.StudentNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.StudentNumber }).IsUnique();
            });
        }

        private void ConfigureDriver(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LicenseNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);
                entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.EmployeeNumber }).IsUnique();
            });
        }

        private void ConfigureVehicle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehicleNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LicensePlate).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Make).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Model).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Color).HasMaxLength(30).IsRequired();
                entity.Property(e => e.VIN).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FuelType).HasMaxLength(30).IsRequired();
                entity.Property(e => e.InsuranceProvider).HasMaxLength(100);
                entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);

                entity.HasIndex(e => e.VehicleNumber).IsUnique();
                entity.HasIndex(e => e.LicensePlate).IsUnique();
                entity.HasIndex(e => e.VIN).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.VehicleNumber }).IsUnique();
            });
        }

        private void ConfigureRoute(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Route>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RouteNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.StartLocation).HasMaxLength(200).IsRequired();
                entity.Property(e => e.EndLocation).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Distance).HasPrecision(10, 2);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.RouteNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.RouteNumber }).IsUnique();
            });
        }

        private void ConfigureRouteStop(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RouteStop>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Latitude).HasPrecision(18, 6);
                entity.Property(e => e.Longitude).HasPrecision(18, 6);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => new { e.RouteId, e.StopOrder }).IsUnique();
            });
        }

        private void ConfigureTrip(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartMileage).HasPrecision(10, 2);
                entity.Property(e => e.EndMileage).HasPrecision(10, 2);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });
        }

        private void ConfigureAttendance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BoardingLocation).HasMaxLength(200);
                entity.Property(e => e.AlightingLocation).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.RecordedBy).HasMaxLength(100).IsRequired();

                entity.HasIndex(e => new { e.StudentId, e.TripId, e.Date }).IsUnique();
            });
        }

        private void ConfigurePayment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PaymentNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.PaymentNumber).IsUnique();
                entity.HasIndex(e => e.TransactionId);
            });
        }

        private void ConfigureMaintenanceRecord(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaintenanceRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaintenanceType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Cost).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
                entity.Property(e => e.ServiceProvider).HasMaxLength(200);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
                entity.Property(e => e.PartsReplaced).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.RefreshToken).HasMaxLength(500);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Username }).IsUnique();
            });
        }

        private void ConfigureNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                entity.Property(e => e.Channel).IsRequired();
                entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
                entity.Property(e => e.EmailError).HasMaxLength(500);
                entity.Property(e => e.SmsError).HasMaxLength(500);
                entity.Property(e => e.Metadata).HasMaxLength(1000);

                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => new { e.TenantId, e.Type });
                entity.HasIndex(e => new { e.TenantId, e.Priority });
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        private void ConfigureConcurrencyControl(ModelBuilder modelBuilder)
        {
            // Configure optimistic concurrency for all entities that inherit from BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();
                }
            }
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Student -> Route (Many-to-One)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Students)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.SetNull);

            // Driver -> Vehicle (One-to-Many)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.AssignedDriver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // Route -> Vehicle (Many-to-One)
            modelBuilder.Entity<Route>()
                .HasOne(r => r.AssignedVehicle)
                .WithMany(v => v.Routes)
                .HasForeignKey(r => r.AssignedVehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Route -> Driver (Many-to-One)
            modelBuilder.Entity<Route>()
                .HasOne(r => r.AssignedDriver)
                .WithMany(d => d.Routes)
                .HasForeignKey(r => r.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // RouteStop -> Route (Many-to-One)
            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.Route)
                .WithMany(r => r.RouteStops)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip -> Route (Many-to-One)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip -> Vehicle (Many-to-One)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip -> Driver (Many-to-One)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance -> Student (Many-to-One)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attendance -> Trip (Many-to-One)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Trip)
                .WithMany(t => t.Attendances)
                .HasForeignKey(a => a.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment -> Student (Many-to-One)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // MaintenanceRecord -> Vehicle (Many-to-One)
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(mr => mr.Vehicle)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(mr => mr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> User (Many-to-One)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

