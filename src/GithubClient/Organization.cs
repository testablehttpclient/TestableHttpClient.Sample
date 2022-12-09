namespace GithubClient;

public record Organization
{
    public required string Login { get; init; }
    public required int Id { get; init; }
    public required Uri Url { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
}
