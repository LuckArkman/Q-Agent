using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Middlewares
{
    /// <summary>
    /// Middleware global encarregado de interceptar todas as exceções não tratadas da aplicação,
    /// registrando-as no logger e emitindo respostas padronizadas sob a RFC 7807 (ProblemDetails).
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uma exceção não tratada foi interceptada pelo barramento global de tratamento de erros.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Tenta recuperar o Correlation ID do middleware anterior para manter a rastreabilidade do laudo de erro
            var correlationId = context.Items.TryGetValue("CorrelationId", out var idObj) ? idObj?.ToString() : context.TraceIdentifier;

            // Monta o objeto ProblemDetails em estrita conformidade com a especificação RFC 7807
            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Ocorreu um erro interno no servidor.",
                Type = "https://q-agent.io/errors/internal-server-error",
                Detail = _env.IsDevelopment() 
                    ? exception.Message // Em ambiente de desenvolvimento, exibe a mensagem curta da exceção
                    : "Desculpe, ocorreu uma falha inesperada em nossos serviços. Por favor, forneça o código de rastreio (TraceId) ao suporte técnico.",
                Instance = context.Request.Path
            };

            // Adiciona extensões para incluir o TraceId/CorrelationId na resposta JSON de forma estruturada
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                problemDetails.Extensions["traceId"] = correlationId;
            }

            // SEGURANÇA ATIVA: Em desenvolvimento, adicionamos a pilha de chamadas (StackTrace), mas NUNCA em produção!
            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(problemDetails, serializerOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
