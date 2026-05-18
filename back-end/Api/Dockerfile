# =========================================================
# Etapa 1: Build & Publish (SDK Image)
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

# Instalar dependências adicionais do Alpine caso necessário
RUN apk add --no-cache icu-libs

# Copiar arquivos de projeto das camadas necessárias para otimizar cache de camadas Docker (restore)
COPY Api/Api.csproj ./Api/
COPY Controllers/Controllers.csproj ./Controllers/
COPY Database/Database.csproj ./Database/
COPY Dtos/Dtos.csproj ./Dtos/
COPY Repositorys/Repositorys.csproj ./Repositorys/
COPY Services/Services.csproj ./Services/

# Executar restore focado no projeto da API Web
RUN dotnet restore Api/Api.csproj

# Copiar o código-fonte restante apenas dos projetos necessários
COPY Api/ ./Api/
COPY Controllers/ ./Controllers/
COPY Database/ ./Database/
COPY Dtos/ ./Dtos/
COPY Repositorys/ ./Repositorys/
COPY Services/ ./Services/

# Compilar e publicar a API Web no diretório de saída
WORKDIR /app/Api
RUN dotnet publish Api.csproj -c Release -o /app/publish --no-restore

# =========================================================
# Etapa 2: Runtime Environment (Minimal Alpine Image)
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Adicionar dependências para suporte a globalização no Alpine (.NET Core exige icu)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expor portas padrões HTTP do ASP.NET Core
EXPOSE 80
EXPOSE 443

# Definir variáveis de ambiente seguras do ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Copiar os artefatos compilados da etapa SDK
COPY --from=build-env /app/publish .

# Segurança Ativa: Criar e rodar o container sob usuário não-root (princípio de privilégio mínimo)
RUN addgroup -S qagentgroup && adduser -S qagentuser -G qagentgroup
USER qagentuser

# Comando de inicialização do container
ENTRYPOINT ["dotnet", "Api.dll"]
