namespace AdminTools.Tests
{
    using AdminTools.interfaces;
    using Moq;

    public abstract class JsonResponseTestBase
    {
        internal Mock<IRequestFactory> RequestFactoryMock { get; set; }
        internal Mock<HttpClient> RequestMock { get; set; }
        internal Mock<HttpResponseMessage> RequestResponse { get; set; }
    }
}