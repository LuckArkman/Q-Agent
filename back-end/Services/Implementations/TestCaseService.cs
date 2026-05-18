using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.TestCase;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe que implementa as regras de negócio para gerenciar cenários individuais, validação de suítes e importação segura em chunks.
    /// </summary>
    public class TestCaseService : ITestCaseService
    {
        private readonly ITestCaseRepository _testCaseRepository;
        private readonly ITestSuiteRepository _suiteRepository;

        public TestCaseService(ITestCaseRepository testCaseRepository, ITestSuiteRepository suiteRepository)
        {
            _testCaseRepository = testCaseRepository ?? throw new ArgumentNullException(nameof(testCaseRepository));
            _suiteRepository = suiteRepository ?? throw new ArgumentNullException(nameof(suiteRepository));
        }

        public async Task<TestCaseResponseDto?> CreateAsync(CreateTestCaseRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar se a suíte de testes realmente existe no PostgreSQL
            var suite = await _suiteRepository.GetByIdAsync(request.TestSuiteId);
            if (suite == null)
                throw new ArgumentException("A suíte de testes associada informada não existe.");

            var testCase = new TestCase
            {
                TestSuiteId = request.TestSuiteId,
                UserPrompt = request.InputPrompt,
                ExpectedAnswer = request.ExpectedOutput
            };

            await _testCaseRepository.AddAsync(testCase);
            var success = await _testCaseRepository.SaveChangesAsync();

            return success ? MapToDto(testCase) : null;
        }

        public async Task<TestCaseResponseDto?> UpdateAsync(Guid id, UpdateTestCaseRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var testCase = await _testCaseRepository.GetByIdAsync(id);
            if (testCase == null)
                return null;

            testCase.UserPrompt = request.InputPrompt;
            testCase.ExpectedAnswer = request.ExpectedOutput;

            _testCaseRepository.Update(testCase);
            var success = await _testCaseRepository.SaveChangesAsync();

            return success ? MapToDto(testCase) : null;
        }

        public async Task<TestCaseResponseDto?> GetByIdAsync(Guid id)
        {
            var testCase = await _testCaseRepository.GetByIdAsync(id);
            if (testCase == null)
                return null;

            return MapToDto(testCase);
        }

        public async Task<IEnumerable<TestCaseResponseDto>> GetByTestSuiteIdAsync(Guid testSuiteId)
        {
            var all = await _testCaseRepository.GetAllAsync();
            var dtos = new List<TestCaseResponseDto>();

            foreach (var testCase in all)
            {
                if (testCase.TestSuiteId == testSuiteId)
                {
                    dtos.Add(MapToDto(testCase));
                }
            }

            return dtos;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var testCase = await _testCaseRepository.GetByIdAsync(id);
            if (testCase == null)
                return false;

            _testCaseRepository.Delete(testCase);
            return await _testCaseRepository.SaveChangesAsync();
        }

        public async Task<BulkImportResultDto> ImportCsvAsync(Guid testSuiteId, Stream csvStream)
        {
            if (csvStream == null)
                throw new ArgumentNullException(nameof(csvStream));

            // Validar se a suíte destino é real antes de processar bytes
            var suite = await _suiteRepository.GetByIdAsync(testSuiteId);
            if (suite == null)
                throw new ArgumentException("A suíte de testes informada para importação não existe.");

            var result = new BulkImportResultDto();
            var listToImport = new List<TestCase>();

            using (var reader = new StreamReader(csvStream, Encoding.UTF8))
            {
                // Ler e ignorar primeira linha caso seja cabeçalho
                var firstLine = await reader.ReadLineAsync();
                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    lineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var columns = ParseCsvLine(line);
                    if (columns.Length < 2)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Linha {lineNumber}: Mapeamento inválido. Esperado 'InputPrompt' e 'ExpectedOutput'.");
                        continue;
                    }

                    var newPrompt = new TestCase
                    {
                        TestSuiteId = testSuiteId,
                        UserPrompt = columns[0].Trim(),
                        ExpectedAnswer = columns[1].Trim()
                    };

                    listToImport.Add(newPrompt);
                }

                // BOAS PRÁTICAS: Processamento em Chunks de 100 para proteção de memória RAM e threads do pool do Postgres
                int chunkSize = 100;
                for (int i = 0; i < listToImport.Count; i += chunkSize)
                {
                    var chunk = listToImport.GetRange(i, Math.Min(chunkSize, listToImport.Count - i));

                    foreach (var item in chunk)
                    {
                        await _testCaseRepository.AddAsync(item);
                    }

                    var success = await _testCaseRepository.SaveChangesAsync();
                    if (success)
                    {
                        result.SuccessCount += chunk.Count;
                    }
                    else
                    {
                        result.ErrorCount += chunk.Count;
                        result.Errors.Add($"Lote {i / chunkSize + 1}: Falha física de transação de dados no PostgreSQL.");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parser resiliente de CSV com suporte inteligente a aspas que escapam vírgulas ou ponto-e-vírgula em prompts complexos.
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var separator = line.Contains(';') ? ';' : ',';
            var result = new List<string>();
            bool inQuotes = false;
            var currentToken = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes; // Inverte o estado de aspas escapadas
                }
                else if (c == separator && !inQuotes)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            result.Add(currentToken.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// Mapeador auxiliar privado para exibição segura de dados na API.
        /// </summary>
        private static TestCaseResponseDto MapToDto(TestCase testCase)
        {
            return new TestCaseResponseDto
            {
                Id = testCase.Id,
                TestSuiteId = testCase.TestSuiteId,
                InputPrompt = testCase.UserPrompt,
                ExpectedOutput = testCase.ExpectedAnswer ?? "",
                CreatedAt = testCase.CreatedAt
            };
        }
    }
}
