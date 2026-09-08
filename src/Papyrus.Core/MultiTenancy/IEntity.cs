namespace Papyrus.Core.MultiTenancy;

/// <summary>
/// Identifies a persisted entity by its unique identifier.
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}
