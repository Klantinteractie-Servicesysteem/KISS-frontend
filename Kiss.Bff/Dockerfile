#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS dotnetbuild
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

FROM node:lts-alpine AS frontend
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
ARG VERSION=0.0.0
ARG REVISION=dev
ENV VERSION=$VERSION REVISION=$REVISION
RUN dotnet publish "Kiss.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:Version=$VERSION /p:SourceRevisionId=$REVISION /p:InformationalVersion=$VERSION+$REVISION

FROM base AS final
WORKDIR /app
COPY --from=dotnetpublish /app/publish .
COPY --from=frontendbuild /app/dist ./wwwroot
ENTRYPOINT ["dotnet", "Kiss.Bff.dll"]