using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.TestSuite;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe que implementa as regras de negócio para gerenciar suítes de testes, agendamentos recorrentes Cron e deleção em cascata.
    /// </summary>
    public class TestSuiteService : ITestSuiteService
    {
        private readonly ITestSuiteRepository _suiteRepository;
        private readonly IAgentConfigRepository _agentRepository;

        public TestSuiteService(ITestSuiteRepository suiteRepository, IAgentConfigRepository agentRepository)
        {
            _suiteRepository = suiteRepository ?? throw new ArgumentNullException(nameof(suiteRepository));
            _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
        }

        public async Task<TestSuiteResponseDto?> CreateAsync(CreateTestSuiteRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar se o bot associado realmente existe no PostgreSQL
            var agent = await _agentRepository.GetByIdAsync(request.AgentConfigId);
            if (agent == null)
                throw new ArgumentException("O Agente de IA associado informado não existe.");

            // Validar expressão Cron se for fornecida
            if (!string.IsNullOrWhiteSpace(request.CronExpression))
            {
                var isCronValid = await ValidateCronExpressionAsync(request.CronExpression);
                if (!isCronValid)
                    throw new ArgumentException("A expressão de agendamento Cron fornecida é sintaticamente inválida.");
            }

            var suite = new TestSuite
            {
                Name = request.Name,
                Description = request.Description,
                CronSchedule = request.CronExpression,
                AgentConfigId = request.AgentConfigId
            };

            await _suiteRepository.AddAsync(suite);
            var success = await _suiteRepository.SaveChangesAsync();

            return success ? MapToDto(suite) : null;
        }

        public async Task<TestSuiteResponseDto?> UpdateAsync(Guid id, UpdateTestSuiteRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var suite = await _suiteRepository.GetByIdAsync(id);
            if (suite == null)
                return null;

            // Validar se o novo bot associado realmente existe no PostgreSQL
            var agent = await _agentRepository.GetByIdAsync(request.AgentConfigId);
            if (agent == null)
                throw new ArgumentException("O novo Agente de IA associado informado não existe.");

            // Validar expressão Cron se for fornecida
            if (!string.IsNullOrWhiteSpace(request.CronExpression))
            {
                var isCronValid = await ValidateCronExpressionAsync(request.CronExpression);
                if (!isCronValid)
                    throw new ArgumentException("A expressão de agendamento Cron fornecida é sintaticamente inválida.");
            }

            suite.Name = request.Name;
            suite.Description = request.Description;
            suite.CronSchedule = request.CronExpression;
            suite.AgentConfigId = request.AgentConfigId;

            _suiteRepository.Update(suite);
            var success = await _suiteRepository.SaveChangesAsync();

            return success ? MapToDto(suite) : null;
        }

        public async Task<TestSuiteResponseDto?> GetByIdAsync(Guid id)
        {
            // Eager-loading ativo desenvolvido na Sprint 9 para extrair os cenários associados
            var suite = await _suiteRepository.GetTestSuiteWithCasesAsync(id);
            if (suite == null)
                return null;

            return MapToDto(suite);
        }

        public async Task<IEnumerable<TestSuiteResponseDto>> GetAllAsync()
        {
            var suites = await _suiteRepository.GetAllAsync();
            var dtos = new List<TestSuiteResponseDto>();

            foreach (var suite in suites)
            {
                // Carrega avidamente cada suíte com seus TestCases para alimentar corretamente o contador no DTO
                var suiteWithCases = await _suiteRepository.GetTestSuiteWithCasesAsync(suite.Id);
                dtos.Add(MapToDto(suiteWithCases ?? suite));
            }

            return dtos;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var suite = await _suiteRepository.GetByIdAsync(id);
            if (suite == null)
                return false;

            // Ao deletar a suíte, o banco de dados propaga a deleção física dos prompts associados (cascade)
            _suiteRepository.Delete(suite);
            return await _suiteRepository.SaveChangesAsync();
        }

        public Task<bool> ValidateCronExpressionAsync(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return Task.FromResult(false);

            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // Expressões Cron padrão aceitam exatamente 5 partes (minuto hora dia mes dia_semana) ou 6 partes (com segundos/anos)
            if (parts.Length != 5 && parts.Length != 6)
                return Task.FromResult(false);

            // Validação sintática por expressão regular: aceita números, *, /, -, , e ?
            var cronRegex = new Regex(@"^[0-9*,/\-?]+$");
            foreach (var part in parts)
            {
                if (!cronRegex.IsMatch(part))
                    return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Mapeador auxiliar privado que converte e popula o contador de cenários de teste de forma otimizada.
        /// </summary>
        private static TestSuiteResponseDto MapToDto(TestSuite suite)
        {
            return new TestSuiteResponseDto
            {
                Id = suite.Id,
                Name = suite.Name,
                Description = suite.Description,
                CronExpression = suite.CronSchedule,
                AgentConfigId = suite.AgentConfigId,
                CreatedAt = suite.CreatedAt,
                TestCasesCount = suite.TestCases?.Count ?? 0
            };
        }
    }
}
