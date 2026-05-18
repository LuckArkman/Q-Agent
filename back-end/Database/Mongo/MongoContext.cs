using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System;

namespace Database.Mongo
{
    /// <summary>
    /// Gerencia a conexão com a base de dados documental MongoDB para persistência de dados de auditoria e históricos.
    /// </summary>
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
            var databaseName = configuration["MongoDB:DatabaseName"] ?? "qa_agent_mongo";
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Obtém a coleção de histórico de avaliações de testes no MongoDB.
        /// </summary>
        public IMongoCollection<EvaluationHistory> EvaluationHistories => 
            _database.GetCollection<EvaluationHistory>("evaluation_histories");
    }
}
