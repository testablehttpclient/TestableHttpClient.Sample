namespace GithubClient;

public class OrganisationNotFoundException : Exception
{
    public OrganisationNotFoundException()
    {
    }

    public OrganisationNotFoundException(string? message) : base(message)
    {
    }

    public OrganisationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
