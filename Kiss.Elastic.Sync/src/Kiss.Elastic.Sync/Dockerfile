FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
RUN addgroup --group kiss --gid 2000 \
    && adduser \    
    --uid 1000 \
    --gid 2000 \
    "kiss" 
RUN chown kiss:kiss  /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj", "src/Kiss.Elastic.Sync/"]
RUN dotnet restore "src/Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj"
COPY . .
WORKDIR "/src/src/Kiss.Elastic.Sync"
RUN dotnet build "Kiss.Elastic.Sync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kiss.Elastic.Sync.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER kiss:kiss
ENTRYPOINT ["dotnet", "Kiss.Elastic.Sync.dll"]
