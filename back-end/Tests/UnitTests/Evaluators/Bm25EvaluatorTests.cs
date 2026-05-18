using System;
using System.Collections.Generic;
using Dtos.Simulation;
using Services.Evaluators;
using Xunit;

namespace Tests.UnitTests.Evaluators
{
    /// <summary>
    /// Classe de testes de unidade para validação de integridade matemática do Bm25Evaluator.
    /// </summary>
    public class Bm25EvaluatorTests
    {
        private readonly Bm25Evaluator _evaluator;

        public Bm25EvaluatorTests()
        {
            _evaluator = new Bm25Evaluator();
        }

        [Fact]
        public void CalculateBm25Score_ShouldReturnZero_WhenResponseIsEmpty()
        {
            // Arrange
            var chunks = new List<RagContextChunkDto>
            {
                new RagContextChunkDto { Content = "Algum contexto de teste." }
            };

            // Act
            var score = _evaluator.CalculateBm25Score(string.Empty, chunks);

            // Assert
            Assert.Equal(0.0, score);
        }

        [Fact]
        public void CalculateBm25Score_ShouldReturnZero_WhenChunksAreEmpty()
        {
            // Arrange
            var chunks = new List<RagContextChunkDto>();

            // Act
            var score = _evaluator.CalculateBm25Score("Uma resposta de agente.", chunks);

            // Assert
            Assert.Equal(0.0, score);
        }

        [Fact]
        public void CalculateBm25Score_ShouldReturnPositiveScore_WhenHighOverlapExists()
        {
            // Arrange
            var response = "O céu é azul devido à dispersão Rayleigh atmosférica.";
            var chunks = new List<RagContextChunkDto>
            {
                new RagContextChunkDto 
                { 
                    Content = "A cor azul do céu é causada pela dispersão Rayleigh da luz solar pelas partículas da atmosfera." 
                }
            };

            // Act
            var score = _evaluator.CalculateBm25Score(response, chunks);

            // Assert
            Assert.True(score > 0.0);
            Assert.True(score <= 1.0);
        }

        [Fact]
        public void CalculateBm25Score_ShouldReturnZero_WhenZeroOverlapExists()
        {
            // Arrange
            var response = "Banana morango abacaxi";
            var chunks = new List<RagContextChunkDto>
            {
                new RagContextChunkDto 
                { 
                    Content = "O desenvolvimento de software ágil utiliza iterações rápidas e entregas contínuas." 
                }
            };

            // Act
            var score = _evaluator.CalculateBm25Score(response, chunks);

            // Assert
            Assert.Equal(0.0, score);
        }
    }
}
