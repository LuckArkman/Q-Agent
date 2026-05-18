# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 15: ServiÃ§o de GestÃ£o de Casos de Testes (TestCaseService)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar lÃ³gica de negÃ³cios para criar, atualizar e importar em massa casos de testes individuais.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Interfaces/ITestCaseService.cs, Services/Implementations/TestCaseService.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Criar operaÃ§Ãµes de cadastro individual de prompts. Implementar parsers de streams de texto para suportar upload e importaÃ§Ã£o rÃ¡pida de mÃºltiplos prompts a partir de arquivos CSV.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Proteger contra estouro de memÃ³ria no upload de grandes lotes de testes, dividindo o lote de importaÃ§Ã£o em subgrupos (chunks) na hora da persistÃªncia no Postgres.

---
*QA Agent Blueprint - Sprint 15 de 45.*
