# Q-Agent - Planejamento TÃ©cnico e Arquitetura de ImplementaÃ§Ã£o
## Sprint 37: Controllers - MÃ©tricas e EvoluÃ§Ã£o Temporal para Dashboards

---

### ðŸŽ¯ 1. Objetivo Geral da Sprint
Desenvolver rotas de agregaÃ§Ã£o analÃ­tica para gerar grÃ¡ficos de evoluÃ§Ã£o temporal no front-end.

---

### ðŸ› ï¸ 2. Pacotes NuGet e DependÃªncias
MongoDB.Driver (v2.24.0)

---

### ðŸ“‚ 3. Arquivos Criados ou Modificados
Controllers/AnalyticsController.cs, Dtos/Analytics/DashboardMetricsDto.cs, Dtos/Analytics/TimelineChartDto.cs

---

### ðŸ“ 4. Detalhamento TÃ©cnico e LÃ³gica de ImplementaÃ§Ã£o
Implementar queries com a engine de agregaÃ§Ã£o ('Aggregate' do MongoDB) para consolidar mÃ©dias diÃ¡rias dos scores RAG (Faithfulness, Relevance, Precision, Recall) e agrupar distribuiÃ§Ãµes em formatos prontos para grÃ¡ficos de barra e pizza.

---

### ðŸ›¡ï¸ 5. Boas PrÃ¡ticas, SeguranÃ§a e EvitaÃ§Ã£o de Erros Comuns
Projetar a agregaÃ§Ã£o apenas dos campos matemÃ¡ticos necessÃ¡rios, ignorando campos de textos longos (como raciocÃ­nio) para acelerar a resposta JSON em 10x.

---
*QA Agent Blueprint - Sprint 37 de 45.*
