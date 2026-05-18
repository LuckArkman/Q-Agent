using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.RAG
{
    /// <summary>
    /// Interface para o serviço encarregado de gerenciar a recuperação, filtragem e estruturação de trechos semânticos de RAG.
    /// </summary>
    public interface IRagRetrievalService
    {
        /// <summary>
        /// Submete um prompt ao mecanismo de busca vetorial assíncrono, aguarda a resposta, realiza o parsing e filtra trechos irrelevantes.
        /// </summary>
        /// <param name="collectionName">Nome da coleção destino do ChromaDB.</param>
        /// <param name="prompt">Texto da busca semântica.</param>
        /// <param name="limit">Número máximo de documentos a retornar.</param>
        /// <param name="maxDistanceThreshold">Distância vetorial máxima tolerável (acima desse limiar, considera-se irrelevante).</param>
        Task<List<RagContextChunkDto>> RetrieveRelevantContextAsync(string collectionName, string prompt, int limit = 5, double maxDistanceThreshold = 1.2);
    }
}
