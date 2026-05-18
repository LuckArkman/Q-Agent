# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 21: ServiÃ§o de Embeddings em C# (EmbeddingGenerator)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar o serviÃ§o de geraÃ§Ã£o de vetores a partir de textos utilizando LLMs remotas ou locais.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Microsoft.SemanticKernel.Connectors.OpenAI (v1.14.0) ou HttpClient nativo

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Embeddings/IEmbeddingGenerator.cs, Services/Embeddings/EmbeddingGenerator.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Escrever conectores assÃ­ncronos HTTP para converter prompts de texto em vetores de decimais ('double[]' ou 'float[]') utilizando APIs OpenAI (text-embedding-ada-002) ou Ollama local (nomic-embed-text).

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Implementar cache local em memÃ³ria RAM de embeddings de strings de prompts de forma a evitar chamadas repetidas e caras de conversÃ£o vetorial no mesmo ciclo.

---
*QA Agent Blueprint - Sprint 21 de 45.*
