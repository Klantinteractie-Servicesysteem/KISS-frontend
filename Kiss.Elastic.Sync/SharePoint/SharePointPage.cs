namespace Kiss.Elastic.Sync.SharePoint;

public record SharePointPage
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyCollection<string> Content { get; init; }
    public required IReadOnlyCollection<string> Headings { get; init; }
    public required string? Url { get; init; }
    public required DateTimeOffset? LastModified { get; init; }
    public required string? CreatedBy { get; init; }
    public required string? LastModifiedBy { get; init; }
}
