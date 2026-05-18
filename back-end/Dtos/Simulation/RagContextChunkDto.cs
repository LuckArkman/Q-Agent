namespace Dtos.Simulation
{
    /// <summary>
    /// Representa um bloco de texto semântico recuperado do banco vetorial ChromaDB com sua respectiva relevância métrica.
    /// </summary>
    public class RagContextChunkDto
    {
        public string ChunkId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Distance { get; set; } // Distância vetorial (Ex: L2, Cosseno)
        public string DocumentId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
