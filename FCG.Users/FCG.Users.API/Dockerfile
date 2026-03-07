FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["FCG.Users.API/FCG.Users.API.csproj", "FCG.Users.API/"]
COPY ["FCG.Users.Application/FCG.Users.Application.csproj", "FCG.Users.Application/"]
COPY ["FCG.Users.Domain/FCG.Users.Domain.csproj", "FCG.Users.Domain/"]
COPY ["FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj", "FCG.Users.Infrastructure/"]

RUN dotnet restore "FCG.Users.API/FCG.Users.API.csproj"

COPY . .

RUN dotnet build "FCG.Users.API/FCG.Users.API.csproj" -c $BUILD_CONFIGURATION --no-restore

RUN dotnet publish "FCG.Users.API/FCG.Users.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-build \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Users.API.dll"]

FROM build AS migrations
WORKDIR /src

RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ef", "database", "update", "--no-build", "--configuration", "Release", "--project", "FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj", "--startup-project", "FCG.Users.API/FCG.Users.API.csproj", "--verbose"]
