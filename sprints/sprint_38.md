# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 38: Logging Centralizado e Rastreabilidade (Serilog)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar estruturaÃ§Ã£o de logs, persistÃªncia em arquivo e rastreamento transacional.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Serilog.AspNetCore (v8.0.1), Serilog.Sinks.File (v5.0.0)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Api/Program.cs, Api/Middlewares/CorrelationIdMiddleware.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Configurar o Serilog em substituiÃ§Ã£o ao logger padrÃ£o do .NET. Criar um middleware para injetar um identificador Ãºnico de correlaÃ§Ã£o (CorrelationId / RequestId) em todas as threads de requisiÃ§Ã£o e correlacionÃ¡-los nos logs estruturados.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
O CorrelationId Ã© vital em microsserviÃ§os. Ele deve ser enviado nos cabeÃ§alhos HTTP ('X-Correlation-ID') de logs e background workers para rastreabilidade de ponta a ponta.

---
*QA Agent Blueprint - Sprint 38 de 45.*
