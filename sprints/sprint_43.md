# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 43: Testes UnitÃ¡rios dos Evaluators e SimulaÃ§Ãµes

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Garantir a precisÃ£o de testes de adaptadores de canais, geradores de embeddings e parsers JSON do LLM Judge.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
xunit (v2.6.6), Moq (v4.20.70)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Tests/UnitTests/Evaluators/FaithfulnessEvaluatorTests.cs, Tests/UnitTests/Adapters/WhatsAppAdapterTests.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Escrever cenÃ¡rios de teste unitÃ¡rio injetando payloads JSON complexos (com erros sintÃ¡ticos e scores flutuantes) para garantir que a classe parser de avaliaÃ§Ã£o os trate de forma robusta sem quebras.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Testar cenÃ¡rios extremos (Edge Cases), como o LLM Judge retornando notas invÃ¡lidas fora do intervalo de 0.0 e 1.0 ou strings corrompidas no lugar do JSON estruturado.

---
*QA Agent Blueprint - Sprint 43 de 45.*
