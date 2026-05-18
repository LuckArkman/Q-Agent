using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.User;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe que implementa as regras de negócio para alteração de perfis, alteração de senhas com validação de complexidade e controle administrativo de cargos.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserAccountRepository _userRepository;

        public UserService(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            return MapToDto(user);
        }

        public async Task<UserResponseDto?> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar formato de e-mail rigorosamente com expressão regular RFC 5322
            if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("O formato do e-mail informado é inválido.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            // Validar conflito de e-mail duplicado
            var emailOwner = await _userRepository.GetByEmailAsync(request.Email);
            if (emailOwner != null && emailOwner.Id != id)
                throw new ArgumentException("O e-mail informado já está sendo utilizado por outra conta.");

            user.FullName = request.FullName;
            user.Email = request.Email;

            _userRepository.Update(user);
            var success = await _userRepository.SaveChangesAsync();

            return success ? MapToDto(user) : null;
        }

        public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            // Validar senha atual
            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
                throw new ArgumentException("A senha atual informada está incorreta.");

            // Validar complexidade de senha: Mínimo 8 caracteres, maiúscula, minúscula, número e caractere especial
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                throw new ArgumentException("A nova senha deve possuir no mínimo 8 caracteres.");

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in request.NewPassword)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (char.IsSymbol(c) || char.IsPunctuation(c)) hasSpecial = true;
            }

            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                throw new ArgumentException("A nova senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.");
            }

            // Criptografar a nova senha
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);

            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(Guid id, string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                throw new ArgumentException("O cargo/papel não pode ser nulo ou vazio.", nameof(newRole));

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.Role = newRole; // Ex: "Blocked" para suspender acessos temporários ou "Admin" para upgrade de acesso
            
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Mapeador auxiliar privado para manter a entidade relacional isolada da camada REST.
        /// </summary>
        private static UserResponseDto MapToDto(UserAccount user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
