using System.Collections.Generic;

namespace Dtos.TestCase
{
    /// <summary>
    /// Contrato de retorno contendo dados analíticos sobre a importação em lote de prompts.
    /// </summary>
    public class BulkImportResultDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
