# =========================
# BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore FCG.Users.API/FCG.Users.API.csproj
RUN dotnet publish FCG.Users.API/FCG.Users.API.csproj -c Release -o /app/publish

# =========================
# RUNTIME (API)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.Users.API.dll"]
