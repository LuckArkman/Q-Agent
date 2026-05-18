using System;
using System.Threading.Tasks;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.User;
using Services.Implementations;
using Moq;
using Xunit;

namespace Tests.UnitTests.Services
{
    /// <summary>
    /// Classe de testes de unidade para validar isoladamente as regras de negócio de UserService.
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserAccountRepository> _userRepoMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserAccountRepository>();
            _userService = new UserService(_userRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.Null(result);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userAccount = new UserAccount
            {
                Id = userId,
                Username = "testuser",
                Email = "test@qagent.io",
                FullName = "Test User",
                Role = "User",
                PasswordHash = "fakehash",
                CreatedAt = DateTime.UtcNow
            };

            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(userAccount);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@qagent.io", result.Email);
            Assert.Equal("Test User", result.FullName);
            Assert.Equal("User", result.Role);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrowArgumentException_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidRequest = new UpdateProfileRequestDto
            {
                FullName = "Updated Name",
                Email = "invalid-email-format"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateProfileAsync(userId, invalidRequest));

            Assert.Equal("O formato do e-mail informado é inválido.", exception.Message);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrowArgumentException_WhenEmailAlreadyInUseByAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            
            var request = new UpdateProfileRequestDto
            {
                FullName = "Updated Name",
                Email = "conflict@qagent.io"
            };

            var existingUser = new UserAccount
            {
                Id = userId,
                Username = "currentuser",
                Email = "old@qagent.io",
                PasswordHash = "hash"
            };

            var conflictingUser = new UserAccount
            {
                Id = otherUserId,
                Username = "otheruser",
                Email = "conflict@qagent.io",
                PasswordHash = "hash"
            };

            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(conflictingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateProfileAsync(userId, request));

            Assert.Equal("O e-mail informado já está sendo utilizado por outra conta.", exception.Message);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _userRepoMock.Verify(repo => repo.GetByEmailAsync(request.Email), Times.Once);
            _userRepoMock.Verify(repo => repo.Update(It.IsAny<UserAccount>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _userService.UpdateUserRoleAsync(userId, "Admin");

            // Assert
            Assert.False(result);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _userRepoMock.Verify(repo => repo.Update(It.IsAny<UserAccount>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_ShouldReturnTrue_WhenRoleIsUpdatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userAccount = new UserAccount
            {
                Id = userId,
                Username = "testuser",
                Email = "test@qagent.io",
                Role = "User",
                PasswordHash = "hash"
            };

            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(userAccount);

            _userRepoMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserRoleAsync(userId, "Admin");

            // Assert
            Assert.True(result);
            Assert.Equal("Admin", userAccount.Role);
            _userRepoMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            _userRepoMock.Verify(repo => repo.Update(userAccount), Times.Once);
            _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
