using Microsoft.EntityFrameworkCore;
using Database.Entities;

namespace Database.Context
{
    /// <summary>
    /// Contexto relacional PostgreSQL responsável pela segurança (UserAccounts) e metadados de suítes de testes.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<AgentConfig> AgentConfigs { get; set; } = null!;
        public DbSet<TestSuite> TestSuites { get; set; } = null!;
        public DbSet<TestCase> TestCases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento de Contas de Usuários no PostgreSQL
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("user_accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(250);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Mapeamento de Configurações do Agente
            modelBuilder.Entity<AgentConfig>(entity =>
            {
                entity.ToTable("agent_configs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ChannelType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EndpointUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ApiKey).HasMaxLength(250);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Mapeamento de Suítes de Testes
            modelBuilder.Entity<TestSuite>(entity =>
            {
                entity.ToTable("test_suites");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CronSchedule).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.AgentConfig)
                    .WithMany(p => p.TestSuites)
                    .HasForeignKey(d => d.AgentConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Mapeamento de Casos de Teste
            modelBuilder.Entity<TestCase>(entity =>
            {
                entity.ToTable("test_cases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserPrompt).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.ExpectedAnswer).HasMaxLength(4000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.TestSuite)
                    .WithMany(p => p.TestCases)
                    .HasForeignKey(d => d.TestSuiteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
