# DatabaseMigrator/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore DatabaseMigrator.csproj
RUN dotnet publish DatabaseMigrator.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .
# SQL-scripts bind-mountes til /app/sql
ENTRYPOINT ["dotnet", "DatabaseMigrator.dll"]
