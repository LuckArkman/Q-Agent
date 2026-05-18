namespace Dtos.User
{
    /// <summary>
    /// Contrato contendo a senha atual e a nova senha para alteração de credenciais.
    /// </summary>
    public class ChangePasswordRequestDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
