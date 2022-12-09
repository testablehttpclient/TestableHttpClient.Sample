namespace GithubClient;

public class GithubApiException : Exception
{
    public GithubApiException()
    {
    }

    public GithubApiException(string? message) : base(message)
    {
    }

    public GithubApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
