FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj", "Kiss.Elastic.Sync/"]
RUN dotnet restore "Kiss.Elastic.Sync/Kiss.Elastic.Sync.csproj"
COPY . .
WORKDIR "/src/Kiss.Elastic.Sync"
RUN dotnet build "Kiss.Elastic.Sync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kiss.Elastic.Sync.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kiss.Elastic.Sync.dll"]
