# =========================================================
# Etapa 1: Build & Publish (SDK Image)
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

# Instalar dependências adicionais do Alpine caso necessário
RUN apk add --no-cache icu-libs

# Copiar arquivos de projeto de todas as camadas para otimizar cache de camadas Docker (restore)
COPY QA-Application.sln ./
COPY Api/Api.csproj ./Api/
COPY Controllers/Controllers.csproj ./Controllers/
COPY Database/Database.csproj ./Database/
COPY Dtos/Dtos.csproj ./Dtos/
COPY Repositorys/Repositorys.csproj ./Repositorys/
COPY Services/Services.csproj ./Services/
COPY SearchService/SearchService.csproj ./SearchService/
COPY WorkerService/WorkerService.csproj ./WorkerService/
COPY Tests/Tests.csproj ./Tests/

# Executar restore de pacotes NuGet de forma consolidada
RUN dotnet restore QA-Application.sln

# Copiar todo o código-fonte restante
COPY . ./

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
