using Microsoft.EntityFrameworkCore.Storage;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Infrastructure.Data;

namespace Rihla.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IRepository<Student>? _students;
        private IRepository<Driver>? _drivers;
        private IRepository<Vehicle>? _vehicles;
        private IRepository<Route>? _routes;
        private IRepository<Trip>? _trips;
        private IRepository<Attendance>? _attendances;
        private IRepository<Payment>? _payments;
        private IRepository<MaintenanceRecord>? _maintenanceRecords;
        private IRepository<User>? _users;
        private IRepository<Notification>? _notifications;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Student> Students => _students ??= new Repository<Student>(_context);
        public IRepository<Driver> Drivers => _drivers ??= new Repository<Driver>(_context);
        public IRepository<Vehicle> Vehicles => _vehicles ??= new Repository<Vehicle>(_context);
        public IRepository<Route> Routes => _routes ??= new Repository<Route>(_context);
        public IRepository<Trip> Trips => _trips ??= new Repository<Trip>(_context);
        public IRepository<Attendance> Attendances => _attendances ??= new Repository<Attendance>(_context);
        public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
        public IRepository<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords ??= new Repository<MaintenanceRecord>(_context);
        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
