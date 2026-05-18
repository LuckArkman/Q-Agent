using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Api.Middlewares
{
    /// <summary>
    /// Middleware responsável por garantir rastreabilidade de ponta a ponta (Correlation ID)
    /// injetando identificadores únicos em cabeçalhos HTTP e contextos estruturados de logging do Serilog.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Tenta recuperar um CorrelationID existente nos cabeçalhos da requisição
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                // Se não existir, gera um novo identificador único UUIDv4
                correlationId = Guid.NewGuid().ToString();
            }

            // Armazena no item de contexto para facilidade de resgate em controllers
            context.Items["CorrelationId"] = correlationId.ToString();

            // Adiciona o Correlation ID nos cabeçalhos de resposta para rastreamento do lado do cliente (observabilidade)
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);
                }
                return Task.CompletedTask;
            });

            // Insere a propriedade estruturada 'CorrelationId' no contexto dinâmico do Serilog (LogContext)
            // de modo que todos os logs gerados na mesma thread de execução herdem este ID automaticamente.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
