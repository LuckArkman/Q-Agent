using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Database.Context;
using Repositorys.Implementations;

namespace Tests.UnitTests.Repositorys
{
    /// <summary>
    /// Classe de testes de unidade para validação de comportamentos isolados do GenericRepository.
    /// </summary>
    public class GenericRepositoryTests
    {
        public class DummyEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var dummyEntity = new DummyEntity { Id = entityId, Name = "Test Entity" };

            var mockDbSet = new Mock<DbSet<DummyEntity>>();
            mockDbSet.Setup(x => x.FindAsync(entityId)).ReturnsAsync(dummyEntity);

            var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockDbContext.Setup(x => x.Set<DummyEntity>()).Returns(mockDbSet.Object);

            var repository = new GenericRepository<DummyEntity>(mockDbContext.Object);

            // Act
            var result = await repository.GetByIdAsync(entityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityId, result.Id);
            Assert.Equal("Test Entity", result.Name);
            mockDbSet.Verify(x => x.FindAsync(entityId), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCallDbSetAdd()
        {
            // Arrange
            var dummyEntity = new DummyEntity { Id = Guid.NewGuid(), Name = "New Entity" };

            var mockDbSet = new Mock<DbSet<DummyEntity>>();
            var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockDbContext.Setup(x => x.Set<DummyEntity>()).Returns(mockDbSet.Object);

            var repository = new GenericRepository<DummyEntity>(mockDbContext.Object);

            // Act
            await repository.AddAsync(dummyEntity);

            // Assert
            mockDbSet.Verify(x => x.AddAsync(dummyEntity, default), Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnTrue_WhenChangesAreSaved()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<DummyEntity>>();
            var mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockDbContext.Setup(x => x.Set<DummyEntity>()).Returns(mockDbSet.Object);
            mockDbContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1); // 1 alteração salva

            var repository = new GenericRepository<DummyEntity>(mockDbContext.Object);

            // Act
            var result = await repository.SaveChangesAsync();

            // Assert
            Assert.True(result);
            mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
