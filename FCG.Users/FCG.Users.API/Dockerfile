FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["FCG.Users.API/FCG.Users.API.csproj", "FCG.Users.API/"]
COPY ["FCG.Users.Application/FCG.Users.Application.csproj", "FCG.Users.Application/"]
COPY ["FCG.Users.Domain/FCG.Users.Domain.csproj", "FCG.Users.Domain/"]
COPY ["FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj", "FCG.Users.Infrastructure/"]

RUN dotnet restore "FCG.Users.API/FCG.Users.API.csproj"
COPY . . 
WORKDIR "/src/FCG.Users.API"
RUN dotnet publish "FCG.Users.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Users.API.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migrations
WORKDIR /src
COPY . .
RUN dotnet tool install --global dotnet-ef --version 8.0.8
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj", "--startup-project", "FCG.Users.API/FCG.Users.API.csproj"]