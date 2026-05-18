using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Classe que implementa o algoritmo BM25 (Best Matching 25) para mensurar a proximidade semântica
    /// entre a resposta do agente de IA e os documentos obtidos do RAG.
    /// </summary>
    public class Bm25Evaluator : IBm25Evaluator
    {
        private readonly double _k1; // Parâmetro de saturação do termo (tipicamente 1.2 a 2.0)
        private readonly double _b;  // Parâmetro de penalização do comprimento (tipicamente 0.75)

        public Bm25Evaluator() : this(1.2, 0.75)
        {
        }

        public Bm25Evaluator(double k1, double b)
        {
            _k1 = k1;
            _b = b;
        }

        public double CalculateBm25Score(string agentResponse, List<RagContextChunkDto> contextChunks)
        {
            if (string.IsNullOrWhiteSpace(agentResponse) || contextChunks == null || contextChunks.Count == 0)
            {
                return 0.0;
            }

            // 1. Tokenizar a resposta do agente (Query)
            var queryTerms = Tokenize(agentResponse);
            if (queryTerms.Count == 0)
            {
                return 0.0;
            }

            // 2. Tokenizar os trechos de contexto RAG (Documentos de Referência)
            var documents = contextChunks
                .Select(c => Tokenize(c.Content ?? string.Empty))
                .Where(d => d.Count > 0)
                .ToList();

            if (documents.Count == 0)
            {
                return 0.0;
            }

            int N = documents.Count;
            double avgdl = documents.Average(d => d.Count);
            double maxScore = 0.0;

            // 3. Iterar sobre cada fatia de documento calculando o score BM25 correspondente
            foreach (var doc in documents)
            {
                double docScore = 0.0;
                int docLength = doc.Count;

                // Frequência de palavras no documento atual
                var termFrequencies = doc
                    .GroupBy(w => w)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var term in queryTerms.Distinct())
                {
                    // f(q_i, D): frequência do termo no documento
                    int f = termFrequencies.ContainsKey(term) ? termFrequencies[term] : 0;

                    if (f > 0)
                    {
                        // n(q_i): número de documentos que contêm o termo
                        int n = documents.Count(d => d.Contains(term));

                        // Calcular a frequência inversa do documento (IDF) de forma amortecida
                        double idf = Math.Log((N - n + 0.5) / (n + 0.5) + 1.0);

                        // Fórmula tradicional do BM25
                        double numerator = f * (_k1 + 1.0);
                        double denominator = f + _k1 * (1.0 - _b + _b * (docLength / avgdl));

                        docScore += idf * (numerator / denominator);
                    }
                }

                maxScore = Math.Max(maxScore, docScore);
            }

            // 4. Normalização de escala logística [0.0, 1.0] usando a tangente hiperbólica (Math.Tanh)
            // para calibrar e limitar o score de forma consistente
            double normalizedProximity = Math.Max(0.0, Math.Min(1.0, Math.Tanh(maxScore / 4.0)));

            return normalizedProximity;
        }

        /// <summary>
        /// Utilitário interno para converter texto em tokens minúsculos sem pontuações.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            // Converte para minúsculo e limpa caracteres especiais/pontuações
            var cleaned = Regex.Replace(text.ToLowerInvariant(), @"[^\w\s]", " ");

            // Separa por blocos de espaços vazios
            return cleaned
                .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(token => token.Length > 1) // Desconsiderar conectivos isolados ou preposições de tamanho 1
                .ToList();
        }
    }
}
