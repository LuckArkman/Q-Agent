# Script do PowerShell para gerar automaticamente os arquivos Markdown das 45 Sprints do Q-Agent
$ErrorActionPreference = "Stop"

# Criar o diretório para as sprints se não existir
$SprintsDir = "i:\Q-Agent\sprints"
if (!(Test-Path -Path $SprintsDir)) {
    New-Item -ItemType Directory -Path $SprintsDir -Force | Out-Null
    Write-Host "Diretório de Sprints criado em $SprintsDir" -ForegroundColor Green
}

# Função auxiliar para gravar o arquivo de cada Sprint com codificação UTF-8
function Write-SprintFile {
    param (
        [int]$Number,
        [string]$Title,
        [string]$Objective,
        [string]$Packages,
        [string]$Files,
        [string]$Details,
        [string]$BestPractices
    )
    
    $Path = Join-Path -Path $SprintsDir -ChildPath ("sprint_" + $Number + ".md")
    
    $MarkdownContent = @"
# Q-Agent - Planejamento Técnico e Arquitetura de Implementação
## Sprint ${Number}: ${Title}

---

### 🎯 1. Objetivo Geral da Sprint
${Objective}

---

### 🛠️ 2. Pacotes NuGet e Dependências
${Packages}

---

### 📂 3. Arquivos Criados ou Modificados
${Files}

---

### 📝 4. Detalhamento Técnico e Lógica de Implementação
${Details}

---

### 🛡️ 5. Boas Práticas, Segurança e Evitação de Erros Comuns
${BestPractices}

---
*QA Agent Blueprint - Sprint ${Number} de 45.*
"@

    # Gravar o arquivo no disco com codificação UTF-8
    Set-Content -Path $Path -Value $MarkdownContent -Encoding utf8
    Write-Host "Sprint $Number gerada com sucesso: $Path" -ForegroundColor Cyan
}

# --- DEFINIÇÕES DAS 45 SPRINTS ---

# Sprints 1 a 5: Setup de Infraestrutura e Arquitetura Base
Write-SprintFile `
    -Number 1 `
    -Title "Setup da Solução e Projetos C# (.NET 8.0)" `
    -Objective "Criar e organizar a solução base e a separação física em 8 projetos de classes e executáveis no .NET 8.0." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "back-end/QA-Application.sln, Api/Api.csproj, Controllers/Controllers.csproj, Database/Database.csproj, Dtos/Dtos.csproj, Repositorys/Repositorys.csproj, Services/Services.csproj, SearchService/SearchService.csproj, WorkerService/WorkerService.csproj" `
    -Details "Estruturação física do repositório utilizando comandos 'dotnet new sln' e 'dotnet new classlib/webapi' para isolar a arquitetura. Mapear referências de dependência de projetos cruzados para respeitar a segregação limpa." `
    -BestPractices "Manter a separação estrita de responsabilidades: a API não acessa diretamente os repositórios; os controllers não acessam o DbContext; e DTOs não possuem lógica de comportamento."

Write-SprintFile `
    -Number 2 `
    -Title "Configuração de Docker Compose para Bancos de Dados" `
    -Objective "Criar o arquivo docker-compose.yml na raiz contendo PostgreSQL 16, MongoDB 6 e ChromaDB para isolar os ambientes locais." `
    -Packages "Nenhum pacote NuGet nesta fase." `
    -Files "docker-compose.yml" `
    -Details "Configuração física de orquestração multi-container no docker-compose. Configurar volumes persistentes mapeados localmente para 'postgres-data', 'mongo-data' e 'chroma-data' para manter integridade física em reinicializações." `
    -BestPractices "Sempre expor portas locais padrão (5432, 27017, 8000), utilizar senhas fortes no ambiente e declarar o restart automático de containers."

Write-SprintFile `
    -Number 3 `
    -Title "Contexto PostgreSQL (AppDbContext) e Entidade UserAccount" `
    -Objective "Configurar o EF Core, o provedor PostgreSQL e mapear a entidade física de contas de usuários no Postgres." `
    -Packages "Microsoft.EntityFrameworkCore (v8.0.2), Npgsql.EntityFrameworkCore.PostgreSQL (v8.0.2)" `
    -Files "Database/Database.csproj, Database/Context/AppDbContext.cs, Database/Entities/UserAccount.cs" `
    -Details "Implementar o DbContext definindo a tabela 'user_accounts'. Mapear indexações exclusivas para Username e Email e aplicar convenções físicas em caixa baixa (snake_case)." `
    -BestPractices "Usar o modificador 'required' do C# 11 para garantir segurança e validação contra nulos. Nunca armazenar senhas em texto puro; apenas hashes gerados por criptografia forte."

Write-SprintFile `
    -Number 4 `
    -Title "Contexto MongoDB (MongoContext) e Entidade EvaluationHistory" `
    -Objective "Configurar a conexão com o MongoDB para armazenar o histórico documental de avaliações de testes." `
    -Packages "MongoDB.Driver (v2.24.0)" `
    -Files "Database/Database.csproj, Database/Mongo/MongoContext.cs, Database/Mongo/EvaluationHistory.cs" `
    -Details "Mapear a classe documental EvaluationHistory utilizando atributos do driver de Bson. Criar o MongoContext injetando IConfiguration para estabelecer e obter a coleção física assíncrona." `
    -BestPractices "Configurar o pool de conexões do MongoDB no DI como Singleton. MongoClient gerencia internamente a concorrência TCP de forma eficiente, reduzindo vazamentos de recursos."

Write-SprintFile `
    -Number 5 `
    -Title "Cliente HTTP ChromaDB (ChromaClient) e Teste de Conectividade" `
    -Objective "Implementar o cliente HTTP nativo em C# para conectar ao banco de dados vetorial ChromaDB." `
    -Packages "System.Net.Http.Json (nativo)" `
    -Files "Database/Chroma/IChromaClient.cs, Database/Chroma/ChromaClient.cs" `
    -Details "Implementar a classe ChromaClient realizando requisições HTTP REST assíncronas aos endpoints '/api/v1/heartbeat' e '/api/v1/collections'. Validar respostas e gerenciar status." `
    -BestPractices "Registrar o ChromaClient através de AddHttpClient para evitar a exaustão de sockets do sistema operacional, permitindo pooling nativo de conexões HTTP."

# Sprints 6 a 10: Padrão de Persistência (Repositories)
Write-SprintFile `
    -Number 6 `
    -Title "Abstração de Repositório Genérico (GenericRepository)" `
    -Objective "Criar o repositório genérico assíncrono para abstrair operações CRUD fundamentais do PostgreSQL no EF Core." `
    -Packages "Microsoft.EntityFrameworkCore (v8.0.2)" `
    -Files "Repositorys/Interfaces/IGenericRepository.cs, Repositorys/Implementations/GenericRepository.cs" `
    -Details "Escrever os métodos genéricos GetByIdAsync, GetAllAsync, AddAsync, Update, Delete e SaveChangesAsync utilizando DbSet genérico do DbContext." `
    -BestPractices "Garantir isolamento do DbContext: a interface IGenericRepository expõe tarefas (Tasks) assíncronas assinalando o comportamento de E/S do banco sem vazar tipos EF Core para fora da biblioteca."

Write-SprintFile `
    -Number 7 `
    -Title "Repositório de Contas de Usuários (UserAccountRepository)" `
    -Objective "Desenvolver o repositório relacional específico para manipulação de contas de usuários no Postgres." `
    -Packages "Microsoft.EntityFrameworkCore (v8.0.2)" `
    -Files "Repositorys/Interfaces/IUserAccountRepository.cs, Repositorys/Implementations/UserAccountRepository.cs" `
    -Details "Implementar métodos assíncronos customizados como GetByUsernameAsync e GetByEmailAsync estendendo a base genérica e realizando consultas otimizadas com FirstOrDefaultAsync." `
    -BestPractices "Sempre buscar por e-mails de forma case-insensitive ou aplicando tratamentos adequados para evitar falsos negativos em cadastros concorrentes."

Write-SprintFile `
    -Number 8 `
    -Title "Repositório de Configuração de Agentes (AgentConfigRepository)" `
    -Objective "Desenvolver o repositório relacional para gerenciar configurações de endpoints e dados dos agentes de IA." `
    -Packages "Microsoft.EntityFrameworkCore (v8.0.2)" `
    -Files "Repositorys/Interfaces/IAgentConfigRepository.cs, Repositorys/Implementations/AgentConfigRepository.cs" `
    -Details "Implementar operações CRUD no Postgres para a classe AgentConfig para cadastrar canais de atendimento, chaves de autenticação e rotas HTTP alvo de simulação." `
    -BestPractices "Proteger chaves de API ('ApiKey') de visualizações indevidas, evitando seu tráfego em consultas que não demandem a credencial física."

Write-SprintFile `
    -Number 9 `
    -Title "Repositórios de Test Suites e Test Cases" `
    -Objective "Implementar repositórios relacionais PostgreSQL para gerenciar as suítes e cenários de testes dos agentes de IA." `
    -Packages "Microsoft.EntityFrameworkCore (v8.0.2)" `
    -Files "Repositorys/Interfaces/ITestSuiteRepository.cs, Repositorys/Implementations/TestSuiteRepository.cs, Repositorys/Interfaces/ITestCaseRepository.cs, Repositorys/Implementations/TestCaseRepository.cs" `
    -Details "Mapear consultas com eager-loading utilizando 'Include' em TestSuiteRepository para extrair uma suíte junto de seus respectivos cenários (TestCases) em uma única operação assíncrona." `
    -BestPractices "Evitar carregar coleções inteiras na memória sem filtros de paginação e rastreamento de estado em cenários massivos (utilizar AsNoTracking em consultas de leitura)."

Write-SprintFile `
    -Number 10 `
    -Title "Repositório documental de Histórico de Avaliações (EvaluationHistoryRepository)" `
    -Objective "Desenvolver o repositório documental para escrita e leitura de métricas do MongoDB." `
    -Packages "MongoDB.Driver (v2.24.0)" `
    -Files "Repositorys/Interfaces/IEvaluationHistoryRepository.cs, Repositorys/Implementations/EvaluationHistoryRepository.cs" `
    -Details "Escrever métodos assíncronos baseados em filtros do driver do Mongo ('Builders.Filter') para extrair históricos por Agente, por Suíte e persistir novos documentos por lote." `
    -BestPractices "Sempre indexar chaves de busca frequentes (AgentConfigId, TestSuiteId e ExecutedAt) no MongoDB para manter performance de consulta instantânea em relatórios históricos."

# Sprints 11 a 15: Camada de Serviços de Domínio (Business Services Base)
Write-SprintFile `
    -Number 11 `
    -Title "Serviço de Autenticação e Segurança (AuthService)" `
    -Objective "Implementar regras de criptografia de senhas, validações de segurança e geração de Tokens JWT." `
    -Packages "BCrypt.Net-Next (v4.0.3), System.IdentityModel.Tokens.Jwt (v8.0.0)" `
    -Files "Services/Interfaces/IAuthService.cs, Services/Implementations/AuthService.cs" `
    -Details "Criar métodos de cadastro e login. Validar credenciais, realizar o hash seguro da senha com BCrypt com fator de custo configurável e emitir tokens JWT assinados com chave simétrica." `
    -BestPractices "Armazenar segredos simétricos de JWT em variáveis de ambiente fora do código e nunca trafegar informações confidenciais nas claims públicas do token."

Write-SprintFile `
    -Number 12 `
    -Title "Serviço de Gestão de Usuários (UserService)" `
    -Objective "Implementar a lógica de negócios de controle cadastral de contas de usuários." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/IUserService.cs, Services/Implementations/UserService.cs" `
    -Details "Criar métodos de gerenciamento de perfil, atualização de senhas e bloqueio de usuários. Validar conflitos de nomes de usuário e e-mails duplicados antes da persistência." `
    -BestPractices "Aplicar validação rigorosa de formato de e-mail e impor regras de complexidade de senha na criação de contas (Mínimo de caracteres, letras, números e especiais)."

Write-SprintFile `
    -Number 13 `
    -Title "Serviço de Gestão de Agentes de IA (AgentConfigService)" `
    -Objective "Implementar regras de negócio para criação e manipulação das configurações de agentes de IA." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/IAgentConfigService.cs, Services/Implementations/AgentConfigService.cs" `
    -Details "Gerenciar o ciclo de vida do agente. Validar acessibilidade de URLs de endpoints e testar chaves de API contra os canais do agente para atestar funcionamento inicial do chatbot." `
    -BestPractices "Tratar timeouts HTTP e falhas de DNS de forma isolada ao validar endpoints para evitar que um chatbot instável trave a thread de cadastro."

Write-SprintFile `
    -Number 14 `
    -Title "Serviço de Gestão de Suítes de Testes (TestSuiteService)" `
    -Objective "Implementar lógica para criar, ativar e monitorar suítes de testes associadas aos agentes." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/ITestSuiteService.cs, Services/Implementations/TestSuiteService.cs" `
    -Details "Regras para gerenciar suítes de teste de integração. Implementar validações de agendamento em formato Cron para garantir compatibilidade sintática." `
    -BestPractices "Validar que o Cron informado é válido através de parsers robustos antes de agendar tarefas assíncronas recorrentes de execução de testes."

Write-SprintFile `
    -Number 15 `
    -Title "Serviço de Gestão de Casos de Testes (TestCaseService)" `
    -Objective "Implementar lógica de negócios para criar, atualizar e importar em massa casos de testes individuais." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/ITestCaseService.cs, Services/Implementations/TestCaseService.cs" `
    -Details "Criar operações de cadastro individual de prompts. Implementar parsers de streams de texto para suportar upload e importação rápida de múltiplos prompts a partir de arquivos CSV." `
    -BestPractices "Proteger contra estouro de memória no upload de grandes lotes de testes, dividindo o lote de importação em subgrupos (chunks) na hora da persistência no Postgres."

# Sprints 16 a 20: Integração com Canais (Chatbot Simulators)
Write-SprintFile `
    -Number 16 `
    -Title "Adaptador Base de Simulação de Mensagens (ChatbotAdapterBase)" `
    -Objective "Criar a infraestrutura abstrata e comum para adaptadores de simulação conversacional." `
    -Packages "System.Net.Http (nativo)" `
    -Files "Services/Adapters/IChatbotAdapter.cs, Services/Adapters/ChatbotAdapterBase.cs" `
    -Details "Definir a interface unificada de envio de mensagens e processamento de webhooks. Implementar o comportamento base de resiliência e medição de latência em milissegundos." `
    -BestPractices "Sempre isolar tempos de latência em milissegundos precisos através da classe Stopwatch, excluindo tempos de overhead internos da máquina executora."

Write-SprintFile `
    -Number 17 `
    -Title "Adaptador de Simulação para WhatsApp" `
    -Objective "Desenvolver o adaptador para simular a entrega de mensagens em webhooks de agentes WhatsApp." `
    -Packages "System.Net.Http (nativo)" `
    -Files "Services/Adapters/WhatsAppChatbotAdapter.cs" `
    -Details "Montar payloads HTTP baseados em estruturas reais do WhatsApp Business Cloud API (mensagens do usuário simuladas). Enviar requisições POST para a URL do agente e processar a resposta." `
    -BestPractices "Garantir suporte à decodificação correta de caracteres especiais e emojis comuns no envio de mensagens de WhatsApp simuladas."

Write-SprintFile `
    -Number 18 `
    -Title "Adaptador de Simulação para Telegram" `
    -Objective "Desenvolver o adaptador para simular a entrega de mensagens em agentes do Telegram." `
    -Packages "System.Net.Http (nativo)" `
    -Files "Services/Adapters/TelegramChatbotAdapter.cs" `
    -Details "Modelar payloads simulando webhooks de mensagens que o Telegram envia ao servidor. Enviar requisições POST com estruturas de bate-papo de IDs de usuários fictícios e capturar as respostas HTTP." `
    -BestPractices "Impor tempos de espera máximos (timeouts) específicos para o Telegram de forma a tratar com resiliência instabilidades do chatbot."

Write-SprintFile `
    -Number 19 `
    -Title "Adaptador de Simulação para APIs Customizadas/Webhooks" `
    -Objective "Desenvolver o adaptador flexível para simular comunicações HTTP contra canais proprietários e webchats." `
    -Packages "System.Net.Http (nativo)" `
    -Files "Services/Adapters/CustomApiChatbotAdapter.cs" `
    -Details "Escrever o cliente HTTP genérico parametrizável que consome payloads estruturados via chaves customizadas de JSON especificadas na configuração do agente." `
    -BestPractices "Garantir tratamento adequado de códigos de erro de protocolo de rede (Ex: 400, 401, 500) para registrar dados de falhas sem travar a suíte inteira."

Write-SprintFile `
    -Number 20 `
    -Title "Serviço de Simulação de Interações Conversacionais" `
    -Objective "Implementar o serviço centralizador de envio de prompts de testes baseado em fábrica de adaptadores (Factory Pattern)." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/IChatbotSimulatorService.cs, Services/Implementations/ChatbotSimulatorService.cs" `
    -Details "Orquestrar o mapeamento do canal selecionado, carregar o adaptador correspondente (WhatsApp, Telegram ou Custom), disparar o fluxo de forma isolada e retornar a resposta do agente com latência associada." `
    -BestPractices "Mapear falhas de rede como respostas vazias associadas a códigos de erro correspondentes, preservando logs limpos de latência para diagnóstico no dashboard."

# Sprints 21 a 25: Integração Vetorial e RAG (Search Engine - SearchService)
Write-SprintFile `
    -Number 21 `
    -Title "Serviço de Embeddings em C# (EmbeddingGenerator)" `
    -Objective "Implementar o serviço de geração de vetores a partir de textos utilizando LLMs remotas ou locais." `
    -Packages "Microsoft.SemanticKernel.Connectors.OpenAI (v1.14.0) ou HttpClient nativo" `
    -Files "Services/Embeddings/IEmbeddingGenerator.cs, Services/Embeddings/EmbeddingGenerator.cs" `
    -Details "Escrever conectores assíncronos HTTP para converter prompts de texto em vetores de decimais ('double[]' ou 'float[]') utilizando APIs OpenAI (text-embedding-ada-002) ou Ollama local (nomic-embed-text)." `
    -BestPractices "Implementar cache local em memória RAM de embeddings de strings de prompts de forma a evitar chamadas repetidas e caras de conversão vetorial no mesmo ciclo."

Write-SprintFile `
    -Number 22 `
    -Title "Serviço de Indexação de Contexto no ChromaDB" `
    -Objective "Desenvolver o serviço responsável por criar coleções de teste e indexar documentos vetoriais base de RAG." `
    -Packages "Nenhum pacote externo (utiliza IChromaClient de Database)" `
    -Files "Services/Interfaces/IChromaIndexingService.cs, Services/Implementations/ChromaIndexingService.cs" `
    -Details "Implementar lógicas para fatiar arquivos de texto de conhecimento e enviá-los de forma estruturada para o ChromaDB contendo ids únicos, embeddings numéricos correspondentes e metadados auxiliares." `
    -BestPractices "Sempre fatiar documentos respeitando limites de tamanho de blocos (chunk size) e sobreposições (overlap) para manter a coerência semântica dos contextos recuperados."

Write-SprintFile `
    -Number 23 `
    -Title "Background Service de Busca Semântica do Search Engine" `
    -Objective "Implementar o Hosted Background Service no projeto SearchService para processar solicitações assíncronas de busca." `
    -Packages "Nenhum pacote externo (BackgroundService nativo)" `
    -Files "SearchService/Program.cs, SearchService/Worker.cs" `
    -Details "Desenvolver a escuta de filas assíncronas (ou canais in-memory). Quando ativado, consome os prompts enviados, interage com o gerador de embeddings e solicita a lista de documentos próximos ao ChromaDB." `
    -BestPractices "Garantir tratamento correto do stoppingToken do C# para que o serviço encerre as requisições ativas de forma graciosa sem deixar threads presas na hora do shutdown."

Write-SprintFile `
    -Number 24 `
    -Title "Recuperação e Formatação de Trechos Semânticos de RAG" `
    -Objective "Desenvolver a lógica de parsing e estruturação das respostas cruas JSON retornadas pelo ChromaDB." `
    -Packages "System.Text.Json (nativo)" `
    -Files "Services/RAG/IRagRetrievalService.cs, Services/RAG/RagRetrievalService.cs" `
    -Details "Ler e mapear metadados, distâncias vetoriais e trechos textuais retornados pelo banco vetorial. Organizar esses dados em uma coleção formatada para consumo direto pelo LLM Judge." `
    -BestPractices "Mapear cenários de ausência de contexto de RAG (Zero matches ou distâncias semânticas absurdas) para classificar o teste com alerta de falta de relevância prévia."

Write-SprintFile `
    -Number 25 `
    -Title "Otimização de Busca Vetorial e Tolerância a Falhas no ChromaDB" `
    -Objective "Implementar tolerância a falhas na camada vetorial, mitigando indisponibilidades e perdas de pacotes." `
    -Packages "Polly (v8.3.0)" `
    -Files "Services/Chroma/ChromaSearchOptimizer.cs" `
    -Details "Envolver buscas e inicializações do ChromaDB em políticas resilientes de repetição (retries) assíncronas e circuit-breaker usando a biblioteca Polly C#." `
    -BestPractices "Estabelecer tempos curtos de timeout de conexão para buscas vetoriais, de forma que o fluxo de avaliação não paralise caso o banco de dados vetorial caia temporariamente."

# Sprints 26 a 30: Mecanismo de Avaliação LLM Judge (Worker Service)
Write-SprintFile `
    -Number 26 `
    -Title "Configuração de Clientes LLM no Worker Service" `
    -Objective "Preparar a infraestrutura de comunicação assíncrona com os provedores de modelos de linguagem inteligência artificial avaliadores (LLM Judge)." `
    -Packages "System.Net.Http (nativo)" `
    -Files "WorkerService/Program.cs, WorkerService/LlmJudgeClient.cs" `
    -Details "Configurar conexões HTTP resilientes com serviços de IA remotos (OpenAI API GPT-4o) ou locais (Ollama Llama 3) para servir como juiz de auditoria de qualidade." `
    -BestPractices "Utilizar a injeção do HttpClientFactory para gerenciar o tráfego pesado de envio de prompts de auditoria, protegendo chaves confidenciais no cofre de configurações."

Write-SprintFile `
    -Number 27 `
    -Title "Engenharia de Prompts para Avaliação de Fidelidade (Faithfulness)" `
    -Objective "Criar o serviço e os templates de prompt de IA especialistas em auditar fidelidade e identificar alucinações nas respostas." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Evaluators/IFaithfulnessEvaluator.cs, Services/Evaluators/FaithfulnessEvaluator.cs" `
    -Details "Implementar o avaliador de fidelidade. Ele envia a Resposta gerada pelo chatbot e o Contexto RAG recuperado, instruindo o LLM Judge a isolar fatos que não possuam suporte direto no material de referência." `
    -BestPractices "Exigir respostas estruturadas em formato JSON estável contendo 'score' de 0 a 1 e um array de justificativa ('reasoning') para auditoria de falhas."

Write-SprintFile `
    -Number 28 `
    -Title "Engenharia de Prompts para Avaliação de Relevância de Resposta (Answer Relevance)" `
    -Objective "Implementar o serviço de avaliação semântica focado em medir se a resposta gerada é relevante e coerente com a dúvida do usuário." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Evaluators/IAnswerRelevanceEvaluator.cs, Services/Evaluators/AnswerRelevanceEvaluator.cs" `
    -Details "Criar e testar prompts que comparam a Pergunta formulada com a Resposta gerada. O LLM Judge avalia se o bot deu voltas (tangenciou) ou se respondeu o cerne da dúvida diretamente." `
    -BestPractices "Garantir que o avaliador penalize adequadamente respostas prontas genéricas evasivas (ex: 'Não sei como te ajudar') quando o contexto possuía a resposta física."

Write-SprintFile `
    -Number 29 `
    -Title "Engenharia de Prompts para Avaliação de Context Precision" `
    -Objective "Implementar o serviço que avalia se os trechos de RAG extraídos do ChromaDB eram de fato precisos e ordenados de forma correta para o prompt." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Evaluators/IContextPrecisionEvaluator.cs, Services/Evaluators/ContextPrecisionEvaluator.cs" `
    -Details "Prompt que avalia o alinhamento da pergunta inicial com os trechos individuais trazidos pelo ChromaDB, medindo a precisão das buscas semânticas internas do agente de IA." `
    -BestPractices "Sempre garantir tratamento robusto para o retorno de scores decimais inválidos gerados por modelos de linguagem instáveis."

Write-SprintFile `
    -Number 30 `
    -Title "Engenharia de Prompts para Avaliação de Context Recall" `
    -Objective "Implementar o serviço que afere a completude da resposta do chatbot comparada à riqueza do contexto disponível." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Evaluators/IContextRecallEvaluator.cs, Services/Evaluators/ContextRecallEvaluator.cs" `
    -Details "Modelar prompt comparativo que valida se o chatbot deixou passar pontos cruciais do contexto que deveriam ter sido citados para satisfazer a pergunta de forma rica." `
    -BestPractices "Orientar a inteligência avaliadora a não aceitar sinonímias fracas que descaracterizem regras de negócios estritas indexadas na base de dados."

# Sprints 31 a 35: Processamento Assíncrono e Endpoints (Controllers)
Write-SprintFile `
    -Number 31 `
    -Title "Processamento Assíncrono do Worker Service (Queue Engine)" `
    -Objective "Implementar no WorkerService a escuta e o processamento de filas e canais de testes assíncronos usando System.Threading.Channels." `
    -Packages "Nenhum pacote externo (Channels nativo)" `
    -Files "WorkerService/Worker.cs, WorkerService/Queue/ITestExecutionQueue.cs, WorkerService/Queue/TestExecutionQueue.cs" `
    -Details "Escrever o motor de fila assíncrona concorrente em memória baseado em 'Channel<T>' para receber interações, gerenciar threads de avaliação sem travar a API principal e disparar o LLM Judge em paralelo." `
    -BestPractices "Evitar vazamento de threads limitando o canal in-memory a uma capacidade estrita (Bounded Channel) e tratando cancelamentos do sistema operacional de forma rápida."

Write-SprintFile `
    -Number 32 `
    -Title "Serviço de Orquestração Geral de Testes (TestOrchestratorService)" `
    -Objective "Implementar a engrenagem que conecta a simulação HTTP de canais, a busca de RAG e a inserção na fila de avaliação de forma assíncrona." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Services/Interfaces/ITestOrchestratorService.cs, Services/Implementations/TestOrchestratorService.cs" `
    -Details "Orquestrar o fluxo. O serviço recebe o disparo, executa a chamada ao chatbot através do IChatbotSimulatorService, em paralelo solicita o contexto semântico ao Search Engine e envia o lote de informações consolidadas para a fila de avaliação do Worker Service." `
    -BestPractices "Usar paralelismo de tarefas de forma segura ('Task.WhenAll') para realizar buscas vetoriais e simulações HTTP de forma simultânea, cortando o tempo total de processamento pela metade."

Write-SprintFile `
    -Number 33 `
    -Title "Controllers - Autenticação e Acesso (AuthController)" `
    -Objective "Implementar endpoints HTTP de Login, Registro e gerenciamento de perfis de segurança." `
    -Packages "Microsoft.AspNetCore.Authentication.JwtBearer (v8.0.2)" `
    -Files "Controllers/AuthController.cs, Dtos/Auth/LoginRequest.cs, Dtos/Auth/RegisterRequest.cs, Dtos/Auth/AuthResponse.cs" `
    -Details "Mapear as rotas '/api/auth/login' e '/api/auth/register'. Processar payloads de entrada seguros, invocar o AuthService para validar hashes e retornar JWT token estruturado." `
    -BestPractices "Validar rigorosamente a entrada através de validações automáticas com DataAnnotations e retornar códigos de status HTTP claros (200 OK, 401 Unauthorized)."

Write-SprintFile `
    -Number 34 `
    -Title "Controllers - Configuração de Agentes (AgentConfigsController)" `
    -Objective "Desenvolver endpoints REST de CRUD para gerenciar as chaves e caminhos dos agentes conversacionais." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Controllers/AgentConfigsController.cs, Dtos/Agent/AgentConfigRequestDto.cs, Dtos/Agent/AgentConfigResponseDto.cs" `
    -Details "Implementar mapeamentos de verbos HTTP GET, POST, PUT, DELETE para gerenciar instâncias de agentes de IA na base Postgres, isolando modelos com DTOs." `
    -BestPractices "Sempre mascarar chaves privadas de segurança ('ApiKey') em retornos GET gerais, retornando apenas indicativos de presença (Ex: '********') por segurança."

Write-SprintFile `
    -Number 35 `
    -Title "Controllers - Suítes e Casos de Testes" `
    -Objective "Desenvolver rotas HTTP de controle e upload em massa de casos de testes de integração." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files "Controllers/TestSuitesController.cs, Controllers/TestCasesController.cs, Dtos/Test/TestSuiteDto.cs, Dtos/Test/TestCaseDto.cs" `
    -Details "Criar endpoints para gerenciar suítes e casos de testes relacionais. Suportar upload de formulários multi-part (IFormFile) para importação de CSVs contendo prompts de auditoria." `
    -BestPractices "Adicionar validação estrita de tipos de arquivos de upload de modo a banir extensões inválidas que possam caracterizar risco à segurança do servidor de arquivos."

# Sprints 36 a 40: Históricos, Dashboards e Observabilidade
Write-SprintFile `
    -Number 36 `
    -Title "Controllers - Históricos e Auditoria de Testes" `
    -Objective "Criar rotas HTTP REST para consulta e auditoria detalhada do histórico de avaliações do MongoDB." `
    -Packages "MongoDB.Driver (v2.24.0)" `
    -Files "Controllers/EvaluationHistoryController.cs, Dtos/History/HistoryFilterDto.cs, Dtos/History/HistoryDetailDto.cs" `
    -Details "Criar rotas GET '/api/history' e '/api/history/{id}' que consultam a base documental MongoDB. Implementar paginação ('Skip'/'Limit') e ordenação decrescente por data de execução." `
    -BestPractices "Sempre paginar listagens de históricos para evitar que o envio de milhões de registros documental trave a memória RAM da aplicação."

Write-SprintFile `
    -Number 37 `
    -Title "Controllers - Métricas e Evolução Temporal para Dashboards" `
    -Objective "Desenvolver rotas de agregação analítica para gerar gráficos de evolução temporal no front-end." `
    -Packages "MongoDB.Driver (v2.24.0)" `
    -Files "Controllers/AnalyticsController.cs, Dtos/Analytics/DashboardMetricsDto.cs, Dtos/Analytics/TimelineChartDto.cs" `
    -Details "Implementar queries com a engine de agregação ('Aggregate' do MongoDB) para consolidar médias diárias dos scores RAG (Faithfulness, Relevance, Precision, Recall) e agrupar distribuições em formatos prontos para gráficos de barra e pizza." `
    -BestPractices "Projetar a agregação apenas dos campos matemáticos necessários, ignorando campos de textos longos (como raciocínio) para acelerar a resposta JSON em 10x."

Write-SprintFile `
    -Number 38 `
    -Title "Logging Centralizado e Rastreabilidade (Serilog)" `
    -Objective "Implementar estruturação de logs, persistência em arquivo e rastreamento transacional." `
    -Packages "Serilog.AspNetCore (v8.0.1), Serilog.Sinks.File (v5.0.0)" `
    -Files "Api/Program.cs, Api/Middlewares/CorrelationIdMiddleware.cs" `
    -Details "Configurar o Serilog em substituição ao logger padrão do .NET. Criar um middleware para injetar um identificador único de correlação (CorrelationId / RequestId) em todas as threads de requisição e correlacioná-los nos logs estruturados." `
    -BestPractices "O CorrelationId é vital em microsserviços. Ele deve ser enviado nos cabeçalhos HTTP ('X-Correlation-ID') de logs e background workers para rastreabilidade de ponta a ponta."

Write-SprintFile `
    -Number 39 `
    -Title "Middleware Global de Tratamento de Exceções" `
    -Objective "Desenvolver o barramento centralizador de capturas de erros de execução do backend." `
    -Packages "Microsoft.AspNetCore.Diagnostics (nativo)" `
    -Files "Api/Middlewares/ExceptionHandlingMiddleware.cs" `
    -Details "Implementar interceptor de exceções no pipeline do ASP.NET. Capturar erros internos de conexão (Postgres offline, timeouts no Chroma, rede inacessível) e convertê-los em respostas seguras e padronizadas no formato 'ProblemDetails' (RFC 7807)." `
    -BestPractices "Nunca expor a stack trace detalhada do erro (linhas de código C# ou senhas cruas de banco) nas respostas HTTP públicas de produção para manter a segurança do ambiente."

Write-SprintFile `
    -Number 40 `
    -Title "Resiliência com Polly e Circuit Breaker HTTP" `
    -Objective "Desenvolver políticas de resiliência e failover nos adaptadores de canais de atendimento e chamadas de APIs de LLMs." `
    -Packages "Polly (v8.3.0), Microsoft.Extensions.Http.Polly (v8.0.2)" `
    -Files "Api/Program.cs, Services/Chroma/PollyPolicies.cs" `
    -Details "Integrar o Polly com HttpClientFactory. Registrar políticas automáticas de retrying com recuo exponencial ('Exponential Backoff') e Circuit Breaker para interromper o envio de testes contra chatbots que estejam fora do ar." `
    -BestPractices "O Circuit Breaker evita desperdício de recursos computacionais: se o chatbot do WhatsApp alvo falhar 5 vezes seguidas, abre o circuito, cancelando os próximos testes instantaneamente sem prender threads."

# Sprints 41 a 45: Validações, CI/CD e Ajustes de Produção
Write-SprintFile `
    -Number 41 `
    -Title "Cache de Alta Performance para Dashboards (Redis/Memory)" `
    -Objective "Implementar mecanismo de caching de dados para otimizar os carregamentos dos gráficos do Dashboard analítico." `
    -Packages "Microsoft.Extensions.Caching.StackExchangeRedis (v8.0.2)" `
    -Files "Api/Program.cs, Services/Cache/ICacheService.cs, Services/Cache/CacheService.cs" `
    -Details "Implementar o serviço de cache híbrido (em memória local ou utilizando Redis). Interceptar consultas analíticas da Dashboard e armazenar resultados pré-calculados por prazos curtos (ex: 5 minutos)." `
    -BestPractices "Sempre invalidar chaves de cache correspondentes ou expirar o tempo de cache quando uma nova suíte de testes for executada com sucesso, garantindo atualização em tempo real."

Write-SprintFile `
    -Number 42 `
    -Title "Testes Unitários das Camadas Base (xUnit + Moq)" `
    -Objective "Escrever a suíte básica de testes unitários automatizados para validar a persistência lógica de dados e serviços do Q-Agent." `
    -Packages "xunit (v2.6.6), Moq (v4.20.70), Microsoft.NET.Test.Sdk (v17.9.0)" `
    -Files "Tests/UnitTests/Services/UserServiceTests.cs, Tests/UnitTests/Repositorys/GenericRepositoryTests.cs" `
    -Details "Montar testes unitários para a camada de negócios e repositório C#. Mockar o DbContext e as interfaces de dados utilizando a biblioteca Moq, atestando o isolamento e validação de regras de domínio." `
    -BestPractices "Os testes de unidade nunca devem depender de conexões reais de bancos ou acessos à internet ativos. Devem rodar de forma puramente isolada em memória."

Write-SprintFile `
    -Number 43 `
    -Title "Testes Unitários dos Evaluators e Simulações" `
    -Objective "Garantir a precisão de testes de adaptadores de canais, geradores de embeddings e parsers JSON do LLM Judge." `
    -Packages "xunit (v2.6.6), Moq (v4.20.70)" `
    -Files "Tests/UnitTests/Evaluators/FaithfulnessEvaluatorTests.cs, Tests/UnitTests/Adapters/WhatsAppAdapterTests.cs" `
    -Details "Escrever cenários de teste unitário injetando payloads JSON complexos (com erros sintáticos e scores flutuantes) para garantir que a classe parser de avaliação os trate de forma robusta sem quebras." `
    -BestPractices "Testar cenários extremos (Edge Cases), como o LLM Judge retornando notas inválidas fora do intervalo de 0.0 e 1.0 ou strings corrompidas no lugar do JSON estruturado."

Write-SprintFile `
    -Number 44 `
    -Title "Configuração de CI/CD e Build Docker Multiestágio" `
    -Objective "Criar os arquivos de pipeline de integração contínua (CI) e o Dockerfile multi-stage otimizado para o ambiente de produção." `
    -Packages "Nenhum pacote externo nesta fase." `
    -Files ".github/workflows/ci.yml, Api/Dockerfile" `
    -Details "Desenvolver o Dockerfile com a técnica de multi-stage build: etapa de compilação SDK limpa de build no .NET 8 e etapa runtime com imagens leves baseadas em Alpine (mínimo vetor de ataque e tamanho otimizado). Criar pipeline do GitHub Actions para rodar builds e suítes de testes a cada push." `
    -BestPractices "Nunca incluir arquivos de segredos (.env, configurações de produção, appsettings privativos) nas imagens públicas e compilações geradas pelo build Docker."

Write-SprintFile `
    -Number 45 `
    -Title "Health Checks, Configuração de Produção e Entrega Final" `
    -Objective "Desenvolver o monitoramento de saúde do backend (.NET Health Checks) e validações finais de ambientes de entrega." `
    -Packages "AspNetCore.HealthChecks.NpgSql (v8.0.0), AspNetCore.HealthChecks.MongoDb (v8.0.0)" `
    -Files "Api/Program.cs, Api/appsettings.Production.json" `
    -Details "Mapear o endpoint '/health' na API. Configurar verificações físicas ativas de integridade e conectividade contra a base PostgreSQL, MongoDB e ChromaDB. Fechar as variáveis finais de ambiente e concluir a liberação do código de backend estável." `
    -BestPractices "As APIs de Health Checks não devem expor segredos nem detalhes de conexões, apenas indicar com simplicidade 'Healthy' ou 'Unhealthy' para orquestradores de nuvem (como Kubernetes)."

Write-Host "Todas as 45 sprints foram geradas e salvas com sucesso em $SprintsDir!" -ForegroundColor Green
