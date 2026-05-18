using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.TestSuite;

namespace Services.Interfaces
{
    /// <summary>
    /// Define as regras de negócio para gerenciar suítes de testes de integração, incluindo validações de agendamento Cron.
    /// </summary>
    public interface ITestSuiteService
    {
        /// <summary>
        /// Cadastra uma nova suíte de testes de integração, validando opcionalmente o agendamento Cron informado.
        /// </summary>
        Task<TestSuiteResponseDto?> CreateAsync(CreateTestSuiteRequestDto request);

        /// <summary>
        /// Edita as configurações cadastrais de uma suíte de testes existente, incluindo validação sintática do Cron.
        /// </summary>
        Task<TestSuiteResponseDto?> UpdateAsync(Guid id, UpdateTestSuiteRequestDto request);

        /// <summary>
        /// Obtém os dados e contador de cenários de uma suíte de testes por Id.
        /// </summary>
        Task<TestSuiteResponseDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Lista todas as suítes de testes cadastradas no sistema.
        /// </summary>
        Task<IEnumerable<TestSuiteResponseDto>> GetAllAsync();

        /// <summary>
        /// Exclui uma suíte de testes do sistema, propagando a exclusão dos cenários associados em cascata.
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Valida sintaticamente se uma expressão Cron de agendamento é compatível com os parsers de segundo plano.
        /// </summary>
        Task<bool> ValidateCronExpressionAsync(string cronExpression);
    }
}
