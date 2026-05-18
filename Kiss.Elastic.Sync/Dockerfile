FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj", "Kiss.Elastic.Sync/"]
RUN dotnet restore "Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj"
COPY ["Kiss.Elastic.Sync", "Kiss.Elastic.Sync"]
WORKDIR "/src/Kiss.Elastic.Sync"
RUN dotnet publish "Kiss.Elastic.Sync.csproj" --no-restore -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kiss.Elastic.Sync.dll"]
