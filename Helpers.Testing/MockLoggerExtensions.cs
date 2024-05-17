using Microsoft.Extensions.Logging;
using Moq;

namespace Helpers.Testing;

public static class MockLoggerExtensions
{
    public static void VerifyWasCalledOnce<T>(
        this Mock<ILogger<T>> self,
        LogLevel logLevel)
    {
        self.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyWasCalledOnce<T>(
        this Mock<ILogger<T>> self,
        LogLevel logLevel,
        params string[] keywords)
    {
        self.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) =>
                    // Do not check for exact matches
                    keywords.All(s => o.ToString().Contains(s, StringComparison.OrdinalIgnoreCase))),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyErrorWasCalledOnce<T>(
        this Mock<ILogger<T>> self)
    {
        self.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    public static void VerifyWasNeverCalled<T>(
        this Mock<ILogger<T>> self)
    {
        self.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception?, string>)It.IsAny<object>()),
            Times.Never);
    }
}