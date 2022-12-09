using System.Net;

using TestableHttpClient;

using static TestableHttpClient.Responses;

namespace GithubClient.Tests;

public class GithubApiClientTests
{
    [Fact]
    public async Task GetOrganization_Timeout_ThrowsGithubApiException()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(Timeout());
        GithubApiClient client = new(handler.CreateClient());

        async Task act() => await client.GetOrganizationAsync("testablehttpclient");
        await Assert.ThrowsAsync<GithubApiException>(act);
        handler.ShouldHaveMadeRequestsTo("https://api.github.com/orgs/testablehttpclient")
            .WithHttpMethod(HttpMethod.Get);
    }

    [Fact]
    public async Task GetOrganization_UnknownOrganisation_ThrowsOrganisationNotFound()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(StatusCode(HttpStatusCode.NotFound));
        HttpClient httpClient = handler.CreateClient();
        GithubApiClient client = new(httpClient);

        async Task act() => await client.GetOrganizationAsync("UnknownOrganization");
        await Assert.ThrowsAsync<OrganisationNotFoundException>(act);
        handler.ShouldHaveMadeRequestsTo("https://api.github.com/orgs/UnknownOrganization")
            .WithHttpMethod(HttpMethod.Get);
    }

    [Fact]
    public async Task GetOrganization_NullResponse_ThrowsOrganisationNotFound()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(Json(null));
        HttpClient httpClient = handler.CreateClient();
        GithubApiClient client = new(httpClient);

        async Task act() => await client.GetOrganizationAsync("testablehttpclient");
        await Assert.ThrowsAsync<OrganisationNotFoundException>(act);

        handler.ShouldHaveMadeRequestsTo("https://api.github.com/orgs/testablehttpclient")
            .WithHttpMethod(HttpMethod.Get);
    }

    [Fact]
    public async Task GetOrganization_ValidResponse_ReturnsOrganisation()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(succesResponse);
        HttpClient httpClient = handler.CreateClient();
        GithubApiClient client = new(httpClient);

        Organization result = await client.GetOrganizationAsync("testablehttpclient");

        Assert.Equal("testablehttpclient", result.Login);
        Assert.Equal("TestableHttpClient", result.Name);

        handler.ShouldHaveMadeRequestsTo("https://api.github.com/orgs/testablehttpclient")
            .WithHttpMethod(HttpMethod.Get);
    }

    private readonly IResponse succesResponse = Json(new
    {
        login = "testablehttpclient",
        id = 116258955,
        node_id = "O_kgDOBu34iw",
        url = "https://api.github.com/orgs/testablehttpclient",
        description = "",
        name = "TestableHttpClient",
        company = (string?)null,
        blog = (string?)null,
        type = "Organization"
    });

    [Fact]
    public async Task GetOrganization_RetryMechanism_ReturnsOrganisation()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(Sequenced(StatusCode(HttpStatusCode.ServiceUnavailable), succesResponse));
        HttpClient httpClient = handler.CreateClient();
        GithubApiClient client = new(httpClient);

        Organization result = await client.GetOrganizationAsync("testablehttpclient");

        Assert.Equal("testablehttpclient", result.Login);
        Assert.Equal("TestableHttpClient", result.Name);

        handler.ShouldHaveMadeRequestsTo("https://api.github.com/orgs/testablehttpclient", 2)
            .WithHttpMethod(HttpMethod.Get);
    }

    [Fact]
    public async Task GetOrganisation_UsingRoute_GetCorrectResponse()
    {
        TestableHttpMessageHandler handler = new();
        handler.RespondWith(Route(builder =>
        {
            builder.Map("/orgs/testablehttpclient", succesResponse);
            builder.MapFallBackResponse(StatusCode(HttpStatusCode.NotFound));
        }));

        HttpClient httpClient = handler.CreateClient();
        GithubApiClient client = new(httpClient);

        async Task act() => await client.GetOrganizationAsync("UnknownOrganization");
        await Assert.ThrowsAsync<OrganisationNotFoundException>(act);

        Organization result = await client.GetOrganizationAsync("testablehttpclient");

        Assert.Equal("testablehttpclient", result.Login);
        Assert.Equal("TestableHttpClient", result.Name);
    }
}
