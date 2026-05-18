# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 32: ServiÃ§o de OrquestraÃ§Ã£o Geral de Testes (TestOrchestratorService)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar a engrenagem que conecta a simulaÃ§Ã£o HTTP de canais, a busca de RAG e a inserÃ§Ã£o na fila de avaliaÃ§Ã£o de forma assÃ­ncrona.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Interfaces/ITestOrchestratorService.cs, Services/Implementations/TestOrchestratorService.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Orquestrar o fluxo. O serviÃ§o recebe o disparo, executa a chamada ao chatbot atravÃ©s do IChatbotSimulatorService, em paralelo solicita o contexto semÃ¢ntico ao Search Engine e envia o lote de informaÃ§Ãµes consolidadas para a fila de avaliaÃ§Ã£o do Worker Service.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Usar paralelismo de tarefas de forma segura ('Task.WhenAll') para realizar buscas vetoriais e simulaÃ§Ãµes HTTP de forma simultÃ¢nea, cortando o tempo total de processamento pela metade.

---
*QA Agent Blueprint - Sprint 32 de 45.*
