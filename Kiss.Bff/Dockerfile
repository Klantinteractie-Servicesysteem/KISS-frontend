#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
RUN addgroup --group kiss --gid 2000 \
    && adduser \    
    --uid 1000 \
    --gid 2000 \
    "kiss" 
RUN chown kiss:kiss  /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 as dotnetbuild
WORKDIR /src
COPY ["Kiss.Bff/Kiss.Bff.csproj", "Kiss.Bff/"]
RUN dotnet restore "Kiss.Bff/Kiss.Bff.csproj"
COPY Kiss.Bff Kiss.Bff
WORKDIR "/src/Kiss.Bff"
RUN dotnet build "Kiss.Bff.csproj" -c Release --no-restore -o /app/build

FROM dotnetbuild as dotnettest
WORKDIR /src
COPY ["Kiss.Bff.Test/Kiss.Bff.Test.csproj", "Kiss.Bff.Test/"]
RUN dotnet restore "Kiss.Bff.Test/Kiss.Bff.Test.csproj"
COPY Kiss.Bff.Test Kiss.Bff.Test
WORKDIR "/src/Kiss.Bff.Test"
RUN dotnet build "Kiss.Bff.Test.csproj" -c Release --no-restore
RUN dotnet test -c Release --no-build --logger trx --results-directory /testresults -v n /p:CollectCoverage=true /p:CoverletOutput=/testresults/ /p:CoverletOutputFormat=lcov; exit 0

FROM node:18-alpine AS frontend
WORKDIR /app
COPY package*.json .
RUN npm ci
COPY . .

FROM frontend as frontendtest
RUN npm run test:unitci -- --reporter junit --outputFile /testresults/vitest-junit.xml

FROM frontend as frontendbuild
RUN npm run build --if-present

FROM scratch as test
COPY --from=dotnettest /testresults/*.trx .
COPY --from=dotnettest /testresults/*.info .
COPY --from=frontendtest /testresults/*.xml .

FROM dotnetbuild AS dotnetpublish
WORKDIR "/src/Kiss.Bff"
RUN dotnet publish "Kiss.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY certificates /usr/local/share/ca-certificates
RUN update-ca-certificates
COPY --from=dotnetpublish /app/publish .
COPY --from=frontendbuild /app/dist ./wwwroot
USER kiss:kiss
ENTRYPOINT ["dotnet", "Kiss.Bff.dll"]