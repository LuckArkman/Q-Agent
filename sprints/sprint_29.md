# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 29: Engenharia de Prompts para AvaliaÃ§Ã£o de Context Precision

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar o serviÃ§o que avalia se os trechos de RAG extraÃ­dos do ChromaDB eram de fato precisos e ordenados de forma correta para o prompt.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Evaluators/IContextPrecisionEvaluator.cs, Services/Evaluators/ContextPrecisionEvaluator.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Prompt que avalia o alinhamento da pergunta inicial com os trechos individuais trazidos pelo ChromaDB, medindo a precisÃ£o das buscas semÃ¢nticas internas do agente de IA.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Sempre garantir tratamento robusto para o retorno de scores decimais invÃ¡lidos gerados por modelos de linguagem instÃ¡veis.

---
*QA Agent Blueprint - Sprint 29 de 45.*
