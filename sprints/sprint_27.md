# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 27: Engenharia de Prompts para AvaliaÃ§Ã£o de Fidelidade (Faithfulness)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Criar o serviÃ§o e os templates de prompt de IA especialistas em auditar fidelidade e identificar alucinaÃ§Ãµes nas respostas.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Evaluators/IFaithfulnessEvaluator.cs, Services/Evaluators/FaithfulnessEvaluator.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Implementar o avaliador de fidelidade. Ele envia a Resposta gerada pelo chatbot e o Contexto RAG recuperado, instruindo o LLM Judge a isolar fatos que nÃ£o possuam suporte direto no material de referÃªncia.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Exigir respostas estruturadas em formato JSON estÃ¡vel contendo 'score' de 0 a 1 e um array de justificativa ('reasoning') para auditoria de falhas.

---
*QA Agent Blueprint - Sprint 27 de 45.*
