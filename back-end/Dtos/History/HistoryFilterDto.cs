using System;

namespace Dtos.History
{
    /// <summary>
    /// DTO contendo filtros de pesquisa, paginação e classificação para auditoria histórica das execuções no MongoDB.
    /// </summary>
    public class HistoryFilterDto
    {
        /// <summary>
        /// O número da página atual (começando em 1).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// O tamanho limite da página de retorno (padrão 10).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Filtro opcional por agente de IA sob teste.
        /// </summary>
        public Guid? AgentConfigId { get; set; }

        /// <summary>
        /// Filtro opcional por suíte de testes de integração.
        /// </summary>
        public Guid? TestSuiteId { get; set; }

        /// <summary>
        /// Filtro opcional de pontuação geral mínima de qualidade (entre 0.0 e 1.0).
        /// </summary>
        public double? MinScore { get; set; }
    }
}
