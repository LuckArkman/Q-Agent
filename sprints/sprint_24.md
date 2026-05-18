# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 24: RecuperaÃ§Ã£o e FormataÃ§Ã£o de Trechos SemÃ¢nticos de RAG

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver a lÃ³gica de parsing e estruturaÃ§Ã£o das respostas cruas JSON retornadas pelo ChromaDB.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
System.Text.Json (nativo)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/RAG/IRagRetrievalService.cs, Services/RAG/RagRetrievalService.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Ler e mapear metadados, distÃ¢ncias vetoriais e trechos textuais retornados pelo banco vetorial. Organizar esses dados em uma coleÃ§Ã£o formatada para consumo direto pelo LLM Judge.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Mapear cenÃ¡rios de ausÃªncia de contexto de RAG (Zero matches ou distÃ¢ncias semÃ¢nticas absurdas) para classificar o teste com alerta de falta de relevÃ¢ncia prÃ©via.

---
*QA Agent Blueprint - Sprint 24 de 45.*
