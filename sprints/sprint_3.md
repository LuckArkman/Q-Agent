# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 3: Contexto PostgreSQL (AppDbContext) e Entidade UserAccount

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Configurar o EF Core, o provedor PostgreSQL e mapear a entidade fÃ­sica de contas de usuÃ¡rios no Postgres.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Microsoft.EntityFrameworkCore (v8.0.2), Npgsql.EntityFrameworkCore.PostgreSQL (v8.0.2)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Database/Database.csproj, Database/Context/AppDbContext.cs, Database/Entities/UserAccount.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Implementar o DbContext definindo a tabela 'user_accounts'. Mapear indexaÃ§Ãµes exclusivas para Username e Email e aplicar convenÃ§Ãµes fÃ­sicas em caixa baixa (snake_case).

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Usar o modificador 'required' do C# 11 para garantir seguranÃ§a e validaÃ§Ã£o contra nulos. Nunca armazenar senhas em texto puro; apenas hashes gerados por criptografia forte.

---
*QA Agent Blueprint - Sprint 3 de 45.*
