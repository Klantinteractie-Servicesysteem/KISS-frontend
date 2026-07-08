FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.0
ARG REVISION=dev
ENV VERSION=$VERSION REVISION=$REVISION
WORKDIR /src
COPY ["Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj", "Kiss.Elastic.Sync/"]
RUN dotnet restore "Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj"
COPY ["Kiss.Elastic.Sync", "Kiss.Elastic.Sync"]
WORKDIR "/src/Kiss.Elastic.Sync"
RUN dotnet publish "Kiss.Elastic.Sync.csproj" --no-restore -c Release -o /app/publish /p:UseAppHost=false /p:Version=$VERSION /p:SourceRevisionId=$REVISION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kiss.Elastic.Sync.dll"]
