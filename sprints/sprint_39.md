# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 39: Middleware Global de Tratamento de ExceÃ§Ãµes

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver o barramento centralizador de capturas de erros de execuÃ§Ã£o do backend.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Microsoft.AspNetCore.Diagnostics (nativo)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Api/Middlewares/ExceptionHandlingMiddleware.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Implementar interceptor de exceÃ§Ãµes no pipeline do ASP.NET. Capturar erros internos de conexÃ£o (Postgres offline, timeouts no Chroma, rede inacessÃ­vel) e convertÃª-los em respostas seguras e padronizadas no formato 'ProblemDetails' (RFC 7807).

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Nunca expor a stack trace detalhada do erro (linhas de cÃ³digo C# ou senhas cruas de banco) nas respostas HTTP pÃºblicas de produÃ§Ã£o para manter a seguranÃ§a do ambiente.

---
*QA Agent Blueprint - Sprint 39 de 45.*
