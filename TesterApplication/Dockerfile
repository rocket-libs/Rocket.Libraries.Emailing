FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY TesterApplication/TesterApplication.csproj TesterApplication/
COPY src/Rocket.Libraries.Emailing/Rocket.Libraries.Emailing.csproj src/Rocket.Libraries.Emailing/
RUN dotnet restore
COPY . .
WORKDIR /src/TesterApplication
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TesterApplication.dll"]
