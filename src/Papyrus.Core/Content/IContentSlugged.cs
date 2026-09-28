namespace Papyrus.Core.Content;

/// <summary>
/// Identifies an aggregate addressed by a slug, which is the public URL
/// segment for that resource.
/// <para>
/// Every implementing aggregate carries a partial unique index on
/// <c>(TenantId, Slug)</c> filtered to <c>"IsDeleted" = false</c>, so a slug
/// collision is a conflict an operator can act on rather than a server fault.
/// </para>
/// </summary>
public interface IContentSlugged
{
    Guid Id { get; set; }

    string Slug { get; set; }
}
