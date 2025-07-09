using Rihla.Core.Entities;

namespace Rihla.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Student> Students { get; }
        IRepository<Driver> Drivers { get; }
        IRepository<Vehicle> Vehicles { get; }
        IRepository<Route> Routes { get; }
        IRepository<Trip> Trips { get; }
        IRepository<Attendance> Attendances { get; }
        IRepository<Payment> Payments { get; }
        IRepository<MaintenanceRecord> MaintenanceRecords { get; }
        IRepository<User> Users { get; }
        IRepository<Notification> Notifications { get; }
        
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
