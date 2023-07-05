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

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
RUN apt-get update && apt-get install unzip
WORKDIR /custom-certs
RUN wget https://developers.kvk.nl/cms/api/uploads/Private_Chain_api_kvk_nl.zip && unzip -j Private_Chain_api_kvk_nl.zip
WORKDIR /src
COPY ["Kiss.Bff/Kiss.Bff.csproj", "Kiss.Bff/"]
RUN dotnet restore "Kiss.Bff/Kiss.Bff.csproj"
COPY Kiss.Bff Kiss.Bff
WORKDIR "/src/Kiss.Bff"
RUN dotnet build "Kiss.Bff.csproj" -c Release --no-restore -o /app/build

FROM build as dotnettest
WORKDIR /src
COPY ["Kiss.Bff.Test/Kiss.Bff.Test.csproj", "Kiss.Bff.Test/"]
RUN dotnet restore "Kiss.Bff.Test/Kiss.Bff.Test.csproj"
COPY Kiss.Bff.Test Kiss.Bff.Test
WORKDIR "/src/Kiss.Bff.Test"
RUN dotnet build "Kiss.Bff.Test.csproj" -c Release --no-restore
ENTRYPOINT dotnet test -c Release --no-build --logger trx --results-directory /testresults -v n

FROM node:18 AS npmtest
WORKDIR /app
COPY package*.json .
RUN npm ci
COPY . .
ENTRYPOINT npm run test:unitci -- --reporter junit --outputFile /testresults/vitest-junit.xml

FROM npmtest as frontend
RUN npm run build --if-present

FROM build AS publish
WORKDIR "/src/Kiss.Bff"
RUN dotnet publish "Kiss.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /custom-certs /usr/local/share/ca-certificates
COPY --from=publish /app/publish .
COPY --from=frontend /app/dist ./wwwroot
RUN update-ca-certificates
USER kiss:kiss
ENTRYPOINT ["dotnet", "Kiss.Bff.dll"]