# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 35: Controllers - SuÃ­tes e Casos de Testes

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver rotas HTTP de controle e upload em massa de casos de testes de integraÃ§Ã£o.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Controllers/TestSuitesController.cs, Controllers/TestCasesController.cs, Dtos/Test/TestSuiteDto.cs, Dtos/Test/TestCaseDto.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Criar endpoints para gerenciar suÃ­tes e casos de testes relacionais. Suportar upload de formulÃ¡rios multi-part (IFormFile) para importaÃ§Ã£o de CSVs contendo prompts de auditoria.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Adicionar validaÃ§Ã£o estrita de tipos de arquivos de upload de modo a banir extensÃµes invÃ¡lidas que possam caracterizar risco Ã  seguranÃ§a do servidor de arquivos.

---
*QA Agent Blueprint - Sprint 35 de 45.*
