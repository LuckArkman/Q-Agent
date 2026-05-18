# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 30: Engenharia de Prompts para AvaliaÃ§Ã£o de Context Recall

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar o serviÃ§o que afere a completude da resposta do chatbot comparada Ã  riqueza do contexto disponÃ­vel.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Evaluators/IContextRecallEvaluator.cs, Services/Evaluators/ContextRecallEvaluator.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Modelar prompt comparativo que valida se o chatbot deixou passar pontos cruciais do contexto que deveriam ter sido citados para satisfazer a pergunta de forma rica.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Orientar a inteligÃªncia avaliadora a nÃ£o aceitar sinonÃ­mias fracas que descaracterizem regras de negÃ³cios estritas indexadas na base de dados.

---
*QA Agent Blueprint - Sprint 30 de 45.*
