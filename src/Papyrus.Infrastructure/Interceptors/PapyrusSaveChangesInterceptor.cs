using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Papyrus.Core.Persistence;

namespace Papyrus.Infrastructure.Interceptors;

public interface IAuditActor
{
    string ActorId { get; }
}

internal sealed class DefaultAuditActor : IAuditActor
{
    public string ActorId => "system";
}

internal sealed class PapyrusSaveChangesInterceptor(IAuditActor auditActor, TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyPolicies(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyPolicies(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void ApplyPolicies(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries().ToArray())
        {
            if (entry.Entity is ISoftDeletable softDelete && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = now;
                softDelete.DeletedBy = auditActor.ActorId;
                softDelete.DeleteReason ??= "Deleted by content command";
            }

            if (entry.Entity is not IAuditedEntity audited)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                audited.CreatedAt = now;
                audited.CreatedBy = auditActor.ActorId;
            }
            else if (entry.State == EntityState.Modified)
            {
                audited.UpdatedAt = now;
                audited.UpdatedBy = auditActor.ActorId;
            }
        }
    }
}

