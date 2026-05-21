using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
