using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.ValueObjects;

namespace SchoolTransportationSystem.Infrastructure.Data
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
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RouteOptimization> RouteOptimizations { get; set; }
        public DbSet<GPSTracking> GPSTrackings { get; set; }
        public DbSet<AttendanceMethod> AttendanceMethods { get; set; }
        public DbSet<PaymentGateway> PaymentGateways { get; set; }
        public DbSet<GeofenceAlert> GeofenceAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            ConfigurePermission(modelBuilder);
            ConfigureRolePermission(modelBuilder);
            ConfigureNotification(modelBuilder);
            ConfigureAuditLog(modelBuilder);
            ConfigureRouteOptimization(modelBuilder);
            ConfigureGPSTracking(modelBuilder);
            ConfigureAttendanceMethod(modelBuilder);
            ConfigurePaymentGateway(modelBuilder);
            ConfigureGeofenceAlert(modelBuilder);

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

        private void ConfigurePermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Resource).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
            });
        }

        private void ConfigureRolePermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();

                entity.HasIndex(e => new { e.Role, e.PermissionId, e.TenantId }).IsUnique();
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

        private void ConfigureAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId);
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(45).IsRequired();
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Success).IsRequired();
                entity.Property(e => e.Details);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.Timestamp);
            });
        }

        private void ConfigureRouteOptimization(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RouteOptimization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OptimizationType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Parameters).HasMaxLength(2000);
                entity.Property(e => e.Results).HasMaxLength(5000);
                entity.Property(e => e.OriginalDistance).HasPrecision(10, 2);
                entity.Property(e => e.OptimizedDistance).HasPrecision(10, 2);
                entity.Property(e => e.OriginalDuration).HasPrecision(10, 2);
                entity.Property(e => e.OptimizedDuration).HasPrecision(10, 2);
                entity.Property(e => e.FuelSavings).HasPrecision(10, 2);
                entity.Property(e => e.CostSavings).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => new { e.RouteId, e.CreatedAt });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OptimizationType);
            });
        }

        private void ConfigureGPSTracking(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GPSTracking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Latitude).HasPrecision(18, 6).IsRequired();
                entity.Property(e => e.Longitude).HasPrecision(18, 6).IsRequired();
                entity.Property(e => e.Altitude).HasPrecision(10, 2);
                entity.Property(e => e.Speed).HasPrecision(10, 2);
                entity.Property(e => e.Heading).HasPrecision(10, 2);
                entity.Property(e => e.Accuracy).HasPrecision(10, 2);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BatteryLevel).HasPrecision(5, 2);
                entity.Property(e => e.SignalStrength).HasPrecision(5, 2);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => new { e.VehicleId, e.Timestamp });
                entity.HasIndex(e => new { e.TripId, e.Timestamp });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Timestamp);
            });
        }

        private void ConfigureAttendanceMethod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MethodType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.RFIDTag).HasMaxLength(50);
                entity.Property(e => e.PhotoPath).HasMaxLength(500);
                entity.Property(e => e.BiometricData).HasMaxLength(2000);
                entity.Property(e => e.QRCode).HasMaxLength(200);
                entity.Property(e => e.Confidence).HasPrecision(5, 2);
                entity.Property(e => e.ProcessingTime).HasPrecision(10, 3);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.Metadata).HasMaxLength(1000);

                entity.HasIndex(e => new { e.AttendanceId, e.MethodType });
                entity.HasIndex(e => e.RFIDTag);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Timestamp);
            });
        }

        private void ConfigurePaymentGateway(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentGateway>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GatewayName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.GatewayType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TransactionId).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ExternalTransactionId).HasMaxLength(200);
                entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.CardLast4).HasMaxLength(4);
                entity.Property(e => e.CardType).HasMaxLength(50);
                entity.Property(e => e.ProcessorResponse).HasMaxLength(1000);
                entity.Property(e => e.SecurityToken).HasMaxLength(500);
                entity.Property(e => e.FraudScore).HasPrecision(5, 2);
                entity.Property(e => e.ProcessingFee).HasPrecision(18, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.ExternalTransactionId);
                entity.HasIndex(e => new { e.PaymentId, e.Status });
                entity.HasIndex(e => e.ProcessedAt);
            });
        }

        private void ConfigureGeofenceAlert(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeofenceAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AlertType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.GeofenceName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CenterLatitude).HasPrecision(18, 6).IsRequired();
                entity.Property(e => e.CenterLongitude).HasPrecision(18, 6).IsRequired();
                entity.Property(e => e.Radius).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.ViolationLatitude).HasPrecision(18, 6);
                entity.Property(e => e.ViolationLongitude).HasPrecision(18, 6);
                entity.Property(e => e.Distance).HasPrecision(10, 2);
                entity.Property(e => e.Severity).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.ActionTaken).HasMaxLength(500);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => new { e.VehicleId, e.AlertType, e.Timestamp });
                entity.HasIndex(e => new { e.TripId, e.Severity });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Timestamp);
            });
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

            // RolePermission -> Permission (Many-to-One)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> User (Many-to-One)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RouteOptimization -> Route (Many-to-One)
            modelBuilder.Entity<RouteOptimization>()
                .HasOne<Route>()
                .WithMany()
                .HasForeignKey(ro => ro.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            // GPSTracking -> Vehicle (Many-to-One)
            modelBuilder.Entity<GPSTracking>()
                .HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(gt => gt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // GPSTracking -> Trip (Many-to-One)
            modelBuilder.Entity<GPSTracking>()
                .HasOne<Trip>()
                .WithMany()
                .HasForeignKey(gt => gt.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            // AttendanceMethod -> Attendance (Many-to-One)
            modelBuilder.Entity<AttendanceMethod>()
                .HasOne<Attendance>()
                .WithMany()
                .HasForeignKey(am => am.AttendanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentGateway -> Payment (Many-to-One)
            modelBuilder.Entity<PaymentGateway>()
                .HasOne<Payment>()
                .WithMany()
                .HasForeignKey(pg => pg.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // GeofenceAlert -> Vehicle (Many-to-One)
            modelBuilder.Entity<GeofenceAlert>()
                .HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(ga => ga.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // GeofenceAlert -> Trip (Many-to-One)
            modelBuilder.Entity<GeofenceAlert>()
                .HasOne<Trip>()
                .WithMany()
                .HasForeignKey(ga => ga.TripId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

