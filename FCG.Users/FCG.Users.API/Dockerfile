# 1. Build & Migrations Bundle
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os projetos
COPY ["FCG.Users.API/FCG.Users.API.csproj", "FCG.Users.API/"]
COPY ["FCG.Users.Application/FCG.Users.Application.csproj", "FCG.Users.Application/"]
COPY ["FCG.Users.Domain/FCG.Users.Domain.csproj", "FCG.Users.Domain/"]

# Restaura
RUN dotnet restore "FCG.Users.API/FCG.Users.API.csproj"

# Copia todo o código
COPY . . 
WORKDIR "/src/FCG.Users.API"

# Compila a aplicação
RUN dotnet build "FCG.Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# --- CORREÇÃO AQUI ---
# Adicionado --version 8.* para evitar erro de pacote NuGet corrompido
RUN dotnet tool install --global dotnet-ef --version 8.*
ENV PATH="$PATH:/root/.dotnet/tools"

# Cria o executável 'efbundle'
RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/build/efbundle
# ---------------------

# 2. Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FCG.Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3. Final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

# Copia o bundle da etapa de build
COPY --from=build /app/build/efbundle .
# Dá permissão de execução
RUN chmod +x ./efbundle

ENTRYPOINT ["dotnet", "FCG.Users.API.dll"]