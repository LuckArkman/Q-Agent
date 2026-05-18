using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.Auth;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe que implementa a criptografia de senhas com BCrypt e a assinatura criptográfica de tokens JWT.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserAccountRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar unicidade do usuário e do e-mail (usando buscas case-insensitive já implementadas na Sprint 7)
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return null; // Username já está em uso

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
                return null; // E-mail já está em uso

            // Criptografar a senha com BCrypt usando o fator de custo recomendado 11 (equilíbrio ideal de CPU/Segurança)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);

            var newUser = new UserAccount
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = "User" // Papel inicial padrão para novos cadastros
            };

            await _userRepository.AddAsync(newUser);
            var success = await _userRepository.SaveChangesAsync();

            if (!success)
                return null;

            var token = GenerateJwtToken(newUser);

            return new AuthResponse
            {
                Token = token,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = newUser.Role
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Obter conta de usuário pelo Username (com suporte a ToLower na Sprint 7)
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                return null; // Usuário não encontrado

            // Validar senha contra o hash criptográfico gravado no Postgres
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                return null; // Senha inválida

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        /// <summary>
        /// Gera e assina criptograficamente um token JWT simétrico de segurança.
        /// </summary>
        private string GenerateJwtToken(UserAccount user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "chave_secreta_super_segura_de_desenvolvimento_com_tamanho_adequado_de_bytes";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "qa_agent_api";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "qa_agent_clients";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Validade padrão de 2 horas para o token de sessão
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
