using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Simulation;
using Services.Evaluators;
using Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Evaluators
{
    /// <summary>
    /// Classe de testes de unidade para validação de comportamentos do FaithfulnessEvaluator.
    /// </summary>
    public class FaithfulnessEvaluatorTests
    {
        private readonly Mock<ILlmJudgeClient> _llmJudgeMock;
        private readonly Mock<ILogger<FaithfulnessEvaluator>> _loggerMock;
        private readonly FaithfulnessEvaluator _evaluator;

        public FaithfulnessEvaluatorTests()
        {
            _llmJudgeMock = new Mock<ILlmJudgeClient>();
            _loggerMock = new Mock<ILogger<FaithfulnessEvaluator>>();
            _evaluator = new FaithfulnessEvaluator(_llmJudgeMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task EvaluateFaithfulnessAsync_ShouldReturnScoreAndBeFaithful_WhenLlmReturnsValidJson()
        {
            // Arrange
            var prompt = "Qual é a cor do céu?";
            var response = "O céu é azul devido à dispersão Rayleigh.";
            var chunks = new List<RagContextChunkDto>
            {
                new RagContextChunkDto { ChunkId = Guid.NewGuid().ToString(), Content = "O céu parece azul por causa da dispersão da luz do sol pelas moléculas da atmosfera.", Source = "atmosfera.txt", Distance = 0.15 }
            };

            var llmResponse = "{\n  \"score\": 0.95,\n  \"reasoning\": [\"A resposta de que o céu é azul está amparada na dispersão atmosférica explicada na Fatia 1.\"]\n}";
            
            _llmJudgeMock
                .Setup(j => j.ExecuteEvaluationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(llmResponse);

            // Act
            var result = await _evaluator.EvaluateFaithfulnessAsync(prompt, response, chunks);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.95, result.Score);
            Assert.True(result.IsFaithful);
            Assert.Single(result.Reasoning);
            Assert.Contains("dispersão atmosférica", result.Reasoning[0]);
            _llmJudgeMock.Verify(j => j.ExecuteEvaluationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task EvaluateFaithfulnessAsync_ShouldCleanMarkdownAndParse_WhenLlmWrapsInBackticks()
        {
            // Arrange
            var prompt = "Prompt";
            var response = "Response";
            var chunks = new List<RagContextChunkDto>();

            var markdownLlmResponse = "```json\n{\n  \"score\": 0.85,\n  \"reasoning\": [\"Válido.\"]\n}\n```";

            _llmJudgeMock
                .Setup(j => j.ExecuteEvaluationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(markdownLlmResponse);

            // Act
            var result = await _evaluator.EvaluateFaithfulnessAsync(prompt, response, chunks);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.85, result.Score);
            Assert.True(result.IsFaithful);
            Assert.Single(result.Reasoning);
            Assert.Equal("Válido.", result.Reasoning[0]);
        }

        [Fact]
        public async Task EvaluateFaithfulnessAsync_ShouldReturnFallback_WhenLlmReturnsMalformedJson()
        {
            // Arrange
            var prompt = "Prompt";
            var response = "Response";
            var chunks = new List<RagContextChunkDto>();

            var corruptedResponse = "Desculpe, não consigo responder em formato JSON hoje.";

            _llmJudgeMock
                .Setup(j => j.ExecuteEvaluationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(corruptedResponse);

            // Act
            var result = await _evaluator.EvaluateFaithfulnessAsync(prompt, response, chunks);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.5, result.Score);
            Assert.False(result.IsFaithful);
            Assert.Contains("JSON mal-formatado retornado pelo LLM Judge. Resposta de fallback aplicada.", result.Reasoning);
        }

        [Fact]
        public async Task EvaluateFaithfulnessAsync_ShouldReturnFailureScore_WhenLlmReturnsEmpty()
        {
            // Arrange
            var prompt = "Prompt";
            var response = "Response";
            var chunks = new List<RagContextChunkDto>();

            _llmJudgeMock
                .Setup(j => j.ExecuteEvaluationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Act
            var result = await _evaluator.EvaluateFaithfulnessAsync(prompt, response, chunks);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.Score);
            Assert.False(result.IsFaithful);
            Assert.Contains("A IA juíza falhou em emitir um parecer.", result.Reasoning);
        }
    }
}
