using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace Helpers.Testing;

public static class MockHttpMessageHandlerExtensions
{
    public static Mock<HttpMessageHandler> WithSuccessfulResponse<T>(
        this Mock<HttpMessageHandler> self,
        T responseContent)
    {
        self
            .GetProtectedSetup()
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            });

        return self;
    }

    public static Mock<HttpMessageHandler> WithUnauthorizedResponse(
        this Mock<HttpMessageHandler> self)
    {
        self
            .GetProtectedSetup()
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                RequestMessage = new HttpRequestMessage()
            });

        return self;
    }

    public static Mock<HttpMessageHandler> WithDelegate(
        this Mock<HttpMessageHandler> self,
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> func)
    {
        self
            .GetProtectedSetup()
            .ReturnsAsync(func);

        return self;
    }

    public static Mock<HttpMessageHandler> WithException<TException>(
        this Mock<HttpMessageHandler> self)
        where TException : Exception, new()
    {
        self
            .GetProtectedSetup()
            .Throws<TException>();

        return self;
    }

    public static HttpClient ToHttpClient(this Mock<HttpMessageHandler> self)
    {
        return new HttpClient(self.Object);
    }

    private static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> GetProtectedSetup(
        this Mock<HttpMessageHandler> self)
    {
        return self
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }
}