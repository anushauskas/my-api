using System.Net;
using CleanArchitecture.Api.Client;

using static CleanArchitecture.Infrastructure.IntegrationTests.Testing;

namespace CleanArchitecture.Infrastructure.IntegrationTests;

[Parallelizable(ParallelScope.All)]
public class AuthorizationTests
{
    static ITodoListsClient _authenticatedClient;
    static ITodoListsClient _notAuthenticatedClient;
    static ITodoListsClient _invalidTokenClient;

    [OneTimeSetUp]
    public static void Init()
    {
        _notAuthenticatedClient = new TodoListsClient(NotAuthenticatedClient);
        _authenticatedClient = new TodoListsClient(AuthenticatedClient);
        _invalidTokenClient = new TodoListsClient(InvalidTokenClientClient);
    }

    [Test]
    public async Task AuthenticatedCall()
    {
        var response = await _authenticatedClient.GetAsync();
        Assert.That(response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

        Assert.That(response.Headers, Contains.Key("Content-Type"));
        Assert.That(response.Headers["Content-Type"], Does.Contain("application/json; charset=utf-8"));

        Assert.That(response.Result.PriorityLevels, Has.Count.EqualTo(4));
    }

    public static object[] AuthenticatedActionsFromUnauthenticatedClients =
    {
        new object[]{
            nameof(_notAuthenticatedClient),
            nameof(_notAuthenticatedClient.GetAsync), 
            async () => await _notAuthenticatedClient.GetAsync() },

        new object[]{
            nameof(_invalidTokenClient),
            nameof(_invalidTokenClient.GetAsync), 
            async () => await _invalidTokenClient.GetAsync() },

        new object[]{
            nameof(_notAuthenticatedClient),
            nameof(_notAuthenticatedClient.CreateAsync), 
            async () => await _notAuthenticatedClient.CreateAsync(new (){ Title = "Title" }) },

        new object[]{
            nameof(_invalidTokenClient),
            nameof(_invalidTokenClient.CreateAsync), 
            async () => await _invalidTokenClient.CreateAsync(new (){ Title = "Title" }) },

        new object[]{
            nameof(_notAuthenticatedClient),
            nameof(_notAuthenticatedClient.UpdateAsync),
            async () => await _notAuthenticatedClient.UpdateAsync(1, new(){ Id = 1, Title = "title" }) },

        new object[]{
            nameof(_invalidTokenClient),
            nameof(_invalidTokenClient.UpdateAsync),
            async () => await _invalidTokenClient.UpdateAsync(1, new(){ Id = 1, Title = "title" }) },

        new object[]{
            nameof(_notAuthenticatedClient),
            nameof(_notAuthenticatedClient.DeleteAsync),
            async () => await _notAuthenticatedClient.DeleteAsync(1) },

        new object[]{
            nameof(_invalidTokenClient),
            nameof(_invalidTokenClient.DeleteAsync),
            async () => await _invalidTokenClient.DeleteAsync(1) },

        new object[]{
            nameof(_notAuthenticatedClient),
            nameof(_notAuthenticatedClient.PurgeAsync),
            async () => await _notAuthenticatedClient.PurgeAsync() },

        new object[]{
            nameof(_invalidTokenClient),
            nameof(_invalidTokenClient.PurgeAsync),
            async () => await _notAuthenticatedClient.PurgeAsync() },
    };

    [TestCaseSource(nameof(AuthenticatedActionsFromUnauthenticatedClients))]
    public void UnauthenticatedCall(string client, string method, Func<Task> act)
    {
        var ex = Assert.ThrowsAsync<ApiException>(async () => await act());
        Assert.That(ex.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }

    [Test]
    public void UnauthorizedCall()
    {
        var act = async () => await _authenticatedClient.PurgeAsync();
        var ex = Assert.ThrowsAsync<ApiException<ProblemDetails>>(async () => await act());
        Assert.That(ex.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        Assert.That(ex.Result.Status, Is.EqualTo((int)HttpStatusCode.Forbidden));
        Assert.That(ex.Result.Title, Is.EqualTo("Forbidden"));
        Assert.That(ex.Result.Type, Is.EqualTo("https://tools.ietf.org/html/rfc7231#section-6.5.3"));
    }
}
