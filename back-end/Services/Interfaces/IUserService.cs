using System;
using System.Threading.Tasks;
using Dtos.User;

namespace Services.Interfaces
{
    /// <summary>
    /// Define as regras de negócio para alteração de perfis, controle de senhas e bloqueio de usuários.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtém o perfil seguro do usuário pelo identificador único.
        /// </summary>
        Task<UserResponseDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Atualiza os dados de contato do perfil do usuário, validando conflitos de e-mails duplicados.
        /// </summary>
        Task<UserResponseDto?> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request);

        /// <summary>
        /// Altera de forma segura a senha do usuário, validando a complexidade e a senha antiga.
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequestDto request);

        /// <summary>
        /// Realiza o bloqueio administrativo ou alteração de cargo do usuário.
        /// </summary>
        Task<bool> UpdateUserRoleAsync(Guid id, string newRole);
    }
}
