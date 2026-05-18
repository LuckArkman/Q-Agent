using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dtos.TestCase;

namespace Services.Interfaces
{
    /// <summary>
    /// Define as regras de negócio para gerenciar cenários/prompts de testes, incluindo importações de arquivos em massa.
    /// </summary>
    public interface ITestCaseService
    {
        /// <summary>
        /// Cadastra um cenário de teste individual em uma suíte, validando os parâmetros.
        /// </summary>
        Task<TestCaseResponseDto?> CreateAsync(CreateTestCaseRequestDto request);

        /// <summary>
        /// Edita um cenário de teste no PostgreSQL.
        /// </summary>
        Task<TestCaseResponseDto?> UpdateAsync(Guid id, UpdateTestCaseRequestDto request);

        /// <summary>
        /// Obtém os dados de um cenário de teste específico.
        /// </summary>
        Task<TestCaseResponseDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Lista todos os cenários pertencentes a uma suíte de testes específica.
        /// </summary>
        Task<IEnumerable<TestCaseResponseDto>> GetByTestSuiteIdAsync(Guid testSuiteId);

        /// <summary>
        /// Exclui um cenário de teste individual.
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Processa em lote e importa múltiplos prompts a partir de um Stream contendo dados formatados em CSV.
        /// </summary>
        Task<BulkImportResultDto> ImportCsvAsync(Guid testSuiteId, Stream csvStream);
    }
}
