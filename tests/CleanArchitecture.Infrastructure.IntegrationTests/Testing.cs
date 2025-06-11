namespace CleanArchitecture.Infrastructure.IntegrationTests;
[SetUpFixture]
public partial class Testing
{
    private static HttpApiFactory _httpApiFactory = null!;
    
    public static HttpClient NotAuthenticatedClient { get; private set; }
    public static HttpClient AuthenticatedClient { get; private set; }
    public static HttpClient InvalidTokenClientClient { get; private set; }

    [OneTimeSetUp]
    public static void RunBeforeAnyTests()
    {
        _httpApiFactory = new HttpApiFactory();
        NotAuthenticatedClient = _httpApiFactory.CreateClient();
        AuthenticatedClient = _httpApiFactory.WithAuthenticationHeader().CreateClient();
        InvalidTokenClientClient = _httpApiFactory.WithSymmetricKey("u8Qw1vQn2Zp3s6X9b4e7h2k5n8r1t4w7y0z3c6f9i2l5o8r1t4w7z0c3f6i9l2p5").WithAuthenticationHeader().CreateClient();
    }

    [OneTimeTearDown]
    public static async Task RunAfterAnyTests()
    {
        await _httpApiFactory.DisposeAsync();
        NotAuthenticatedClient.Dispose();
        AuthenticatedClient.Dispose();
        InvalidTokenClientClient.Dispose();
    }
}
