# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 34: Controllers - ConfiguraÃ§Ã£o de Agentes (AgentConfigsController)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver endpoints REST de CRUD para gerenciar as chaves e caminhos dos agentes conversacionais.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Controllers/AgentConfigsController.cs, Dtos/Agent/AgentConfigRequestDto.cs, Dtos/Agent/AgentConfigResponseDto.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Implementar mapeamentos de verbos HTTP GET, POST, PUT, DELETE para gerenciar instÃ¢ncias de agentes de IA na base Postgres, isolando modelos com DTOs.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Sempre mascarar chaves privadas de seguranÃ§a ('ApiKey') em retornos GET gerais, retornando apenas indicativos de presenÃ§a (Ex: '********') por seguranÃ§a.

---
*QA Agent Blueprint - Sprint 34 de 45.*
