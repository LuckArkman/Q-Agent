# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 40: ResiliÃªncia com Polly e Circuit Breaker HTTP

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver polÃ­ticas de resiliÃªncia e failover nos adaptadores de canais de atendimento e chamadas de APIs de LLMs.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Polly (v8.3.0), Microsoft.Extensions.Http.Polly (v8.0.2)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Api/Program.cs, Services/Chroma/PollyPolicies.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Integrar o Polly com HttpClientFactory. Registrar polÃ­ticas automÃ¡ticas de retrying com recuo exponencial ('Exponential Backoff') e Circuit Breaker para interromper o envio de testes contra chatbots que estejam fora do ar.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
O Circuit Breaker evita desperdÃ­cio de recursos computacionais: se o chatbot do WhatsApp alvo falhar 5 vezes seguidas, abre o circuito, cancelando os prÃ³ximos testes instantaneamente sem prender threads.

---
*QA Agent Blueprint - Sprint 40 de 45.*
