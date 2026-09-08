using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// Centralizes the provider-aware search and tag predicates used by the public
/// content adapter. In-memory test databases cannot translate PostgreSQL ILIKE,
/// so the predicate falls back to case-insensitive containment for those hosts.
/// </summary>
public static class PublicSearchFilter
{
    public static System.Linq.Expressions.Expression<Func<BlogPost, bool>> BlogPostSearch(string term, bool inMemory) =>
        inMemory
            ? post => post.Title.ToLowerInvariant().Contains(term.ToLowerInvariant())
                || post.Summary.ToLowerInvariant().Contains(term.ToLowerInvariant())
                || post.ContentMarkdown.ToLowerInvariant().Contains(term.ToLowerInvariant())
                || post.BlogPostTags.Any(link => link.Tag.Name.ToLowerInvariant().Contains(term.ToLowerInvariant()) || link.Tag.Slug.ToLowerInvariant().Contains(term.ToLowerInvariant()))
            : post => EF.Functions.ILike(post.Title, $"%{term}%")
                || EF.Functions.ILike(post.Summary, $"%{term}%")
                || EF.Functions.ILike(post.ContentMarkdown, $"%{term}%")
                || post.BlogPostTags.Any(link => EF.Functions.ILike(link.Tag.Name, $"%{term}%") || EF.Functions.ILike(link.Tag.Slug, $"%{term}%"));

    public static System.Linq.Expressions.Expression<Func<BlogPost, bool>> BlogPostTag(string tag, bool inMemory) =>
        inMemory
            ? post => post.BlogPostTags.Any(link => link.Tag.Slug.ToLowerInvariant() == tag.ToLowerInvariant() || link.Tag.Name.ToLowerInvariant() == tag.ToLowerInvariant())
            : post => post.BlogPostTags.Any(link => EF.Functions.ILike(link.Tag.Slug, tag) || EF.Functions.ILike(link.Tag.Name, tag));

    public static System.Linq.Expressions.Expression<Func<Project, bool>> ProjectSearch(string term, bool inMemory) =>
        inMemory
            ? project => project.Title.ToLowerInvariant().Contains(term.ToLowerInvariant())
                || project.Summary.ToLowerInvariant().Contains(term.ToLowerInvariant())
                || (project.Description ?? string.Empty).ToLowerInvariant().Contains(term.ToLowerInvariant())
                || project.ProjectTags.Any(link => link.Tag.Name.ToLowerInvariant().Contains(term.ToLowerInvariant()) || link.Tag.Slug.ToLowerInvariant().Contains(term.ToLowerInvariant()))
            : project => EF.Functions.ILike(project.Title, $"%{term}%")
                || EF.Functions.ILike(project.Summary, $"%{term}%")
                || EF.Functions.ILike(project.Description ?? string.Empty, $"%{term}%")
                || project.ProjectTags.Any(link => EF.Functions.ILike(link.Tag.Name, $"%{term}%") || EF.Functions.ILike(link.Tag.Slug, $"%{term}%"));

    public static System.Linq.Expressions.Expression<Func<Project, bool>> ProjectTag(string tag, bool inMemory) =>
        inMemory
            ? project => project.ProjectTags.Any(link => link.Tag.Slug.ToLowerInvariant() == tag.ToLowerInvariant() || link.Tag.Name.ToLowerInvariant() == tag.ToLowerInvariant())
            : project => project.ProjectTags.Any(link => EF.Functions.ILike(link.Tag.Slug, tag) || EF.Functions.ILike(link.Tag.Name, tag));
}
