using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Services.Adapters;

namespace Tests.UnitTests.Adapters
{
    /// <summary>
    /// Classe de testes de unidade para validação de comportamentos do WhatsAppChatbotAdapter.
    /// </summary>
    public class WhatsAppAdapterTests
    {
        [Fact]
        public async Task SendMessageAsync_ShouldReturnSuccess_WhenChatbotResponds200OkWithJson()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>(); // Comportamento Loose para ignorar chamadas implícitas como Dispose
            
            var responseContent = "{\"response\": \"Olá! Como posso ajudar você hoje?\"}";
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            
            var mockClientFactory = new Mock<IHttpClientFactory>();
            mockClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var adapter = new WhatsAppChatbotAdapter(mockClientFactory.Object);

            // Act
            var result = await adapter.SendMessageAsync("https://fake-chatbot.com/webhook", "test-api-key", "Oi, bot!");

            // Assert
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("Olá! Como posso ajudar você hoje?", result.ResponseText);
            Assert.True(result.LatencyMs >= 0);
            
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri == new Uri("https://fake-chatbot.com/webhook") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == "test-api-key"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendMessageAsync_ShouldReturnText_WhenChatbotResponds200OkWithRawText()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Resposta em texto puro!")
            };

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var mockClientFactory = new Mock<IHttpClientFactory>();
            mockClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var adapter = new WhatsAppChatbotAdapter(mockClientFactory.Object);

            // Act
            var result = await adapter.SendMessageAsync("https://fake-chatbot.com/webhook", string.Empty, "Oi, bot!");

            // Assert
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("Resposta em texto puro!", result.ResponseText);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldReturnFailure_WhenChatbotRespondsErrorStatus()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var mockClientFactory = new Mock<IHttpClientFactory>();
            mockClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var adapter = new WhatsAppChatbotAdapter(mockClientFactory.Object);

            // Act
            var result = await adapter.SendMessageAsync("https://fake-chatbot.com/webhook", string.Empty, "Oi, bot!");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("HTTP de erro: 500", result.ErrorMessage);
        }
    }
}
