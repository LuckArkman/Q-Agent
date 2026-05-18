# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 28: Engenharia de Prompts para AvaliaÃ§Ã£o de RelevÃ¢ncia de Resposta (Answer Relevance)

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Implementar o serviÃ§o de avaliaÃ§Ã£o semÃ¢ntica focado em medir se a resposta gerada Ã© relevante e coerente com a dÃºvida do usuÃ¡rio.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
Nenhum pacote externo nesta fase.

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Services/Evaluators/IAnswerRelevanceEvaluator.cs, Services/Evaluators/AnswerRelevanceEvaluator.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Criar e testar prompts que comparam a Pergunta formulada com a Resposta gerada. O LLM Judge avalia se o bot deu voltas (tangenciou) ou se respondeu o cerne da dÃºvida diretamente.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Garantir que o avaliador penalize adequadamente respostas prontas genÃ©ricas evasivas (ex: 'NÃ£o sei como te ajudar') quando o contexto possuÃ­a a resposta fÃ­sica.

---
*QA Agent Blueprint - Sprint 28 de 45.*
