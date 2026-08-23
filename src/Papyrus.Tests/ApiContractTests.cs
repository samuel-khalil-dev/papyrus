using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Papyrus.Contracts.Content;

namespace Papyrus.Tests;

public sealed class ApiContractTests
{
    [Fact]
    public void Upsert_contracts_expose_required_and_length_validation_metadata()
    {
        var slug = typeof(ProjectUpsertRequest).GetProperty(nameof(ProjectUpsertRequest.Slug))!;
        var title = typeof(ProjectUpsertRequest).GetProperty(nameof(ProjectUpsertRequest.Title))!;
        var summary = typeof(ProjectUpsertRequest).GetProperty(nameof(ProjectUpsertRequest.Summary))!;

        Assert.NotEmpty(slug.GetCustomAttributes(typeof(RequiredAttribute), true));
        Assert.Equal(200, slug.GetCustomAttributes(typeof(MaxLengthAttribute), true).Cast<MaxLengthAttribute>().Single().Length);
        Assert.NotEmpty(title.GetCustomAttributes(typeof(RequiredAttribute), true));
        Assert.Equal(500, summary.GetCustomAttributes(typeof(MaxLengthAttribute), true).Cast<MaxLengthAttribute>().Single().Length);
    }

    [Fact]
    public void Versioned_page_contract_serializes_as_camel_case()
    {
        var response = new PageResponse<ProjectSummaryResponse>(
            [new ProjectSummaryResponse(Guid.Empty, "papyrus", "Papyrus", "Summary", null, false, true, null)],
            1, 20, 1, 1);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"items\"", json, StringComparison.Ordinal);
        Assert.Contains("\"pageSize\"", json, StringComparison.Ordinal);
        Assert.Contains("\"totalPages\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Items\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Public_detail_contract_does_not_contain_tenant_or_deletion_fields()
    {
        var propertyNames = typeof(PublicProjectDetail).GetProperties().Select(property => property.Name).ToArray();

        Assert.DoesNotContain(nameof(PublicProjectDetail).Replace("PublicProjectDetail", "TenantId"), propertyNames);
        Assert.DoesNotContain("IsDeleted", propertyNames);
        Assert.DoesNotContain("DeleteReason", propertyNames);
    }
}
