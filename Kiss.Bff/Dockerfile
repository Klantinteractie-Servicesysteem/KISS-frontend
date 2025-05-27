#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
RUN addgroup --group kiss --gid 2000 \
    && adduser \    
    --uid 1000 \
    --gid 2000 \
    "kiss" 
RUN chown kiss:kiss  /app

# Security updates - upgrade kwetsbare packages naar nieuwste beschikbare versies
# Note: zlib1g CVE-2023-45853 is gemarkeerd als "ignored" door het Debian security team
# omdat het het MiniZip component betreft dat niet wordt gebouwd/geleverd met het zlib1g package die wij gebruiken
# Bron: https://security-tracker.debian.org/tracker/CVE-2023-45853
RUN apt-get update && \
   apt-get upgrade -y \
       zlib1g \
       libc-bin \
       libc6 \
       perl-base \
       util-linux \
       tar \
       libsystemd0 \
       passwd \
       libpam0g \
       libtinfo6 \
       libgcrypt20 \
       libgnutls30 \
       gpgv \
       libstdc++6 \
       coreutils \
       apt && \
   apt-get clean && \
   rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnetbuild
WORKDIR /src
COPY ["Kiss.Bff/Kiss.Bff.csproj", "Kiss.Bff/"]
RUN dotnet restore "Kiss.Bff/Kiss.Bff.csproj"
COPY .git .git
COPY Kiss.Bff Kiss.Bff
WORKDIR "/src/Kiss.Bff"
RUN dotnet build "Kiss.Bff.csproj" -c Release --no-restore -o /app/build

FROM dotnetbuild AS dotnettest
WORKDIR /src
COPY ["Kiss.Bff.Test/Kiss.Bff.Test.csproj", "Kiss.Bff.Test/"]
RUN dotnet restore "Kiss.Bff.Test/Kiss.Bff.Test.csproj"
COPY Kiss.Bff.Test Kiss.Bff.Test
WORKDIR "/src/Kiss.Bff.Test"
RUN dotnet build "Kiss.Bff.Test.csproj" -c Release --no-restore
RUN dotnet test -c Release --no-build --logger trx --results-directory /testresults -v n

FROM node:18-alpine AS frontend
WORKDIR /app
COPY package*.json .
RUN npm ci
COPY . .

FROM frontend AS frontendtest
RUN npm run test:unitci -- --reporter junit --outputFile /testresults/vitest-junit.xml

FROM frontend AS frontendbuild
RUN npm run build --if-present

FROM scratch AS test
COPY --from=dotnettest /testresults/*.trx .
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