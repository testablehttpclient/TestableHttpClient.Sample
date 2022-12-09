using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;

namespace GithubClient;

public class GithubApiClient
{
    private readonly HttpClient httpClient;

    public GithubApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<Organization> GetOrganizationAsync(string organisationName) => GetOrganizationAsync(organisationName, CancellationToken.None);

    public async Task<Organization> GetOrganizationAsync(string organisationName, CancellationToken cancellationToken)
    {
        int retryCount = 3;

        while (retryCount-- >= 0)
        {
            try
            {
                var result = await httpClient.GetAsync($"https://api.github.com/orgs/{organisationName}", cancellationToken);

                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new OrganisationNotFoundException();
                }
                else if (result.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    continue;
                }
                return (await result.Content.ReadFromJsonAsync<Organization>()) ?? throw new OrganisationNotFoundException();
            }
            catch (TaskCanceledException)
            {
                throw new GithubApiException("An error occured on Github, please try again later.");
            }
        }

        throw new GithubApiException("An error occured on Github, please try again later.");
    }
}
