FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG GITHUB_USERNAME
ARG GITHUB_TOKEN

WORKDIR /src

# Adicionar fonte do GitHub Packages
RUN dotnet nuget add source "https://nuget.pkg.github.com/TC-FIAP-Grupo-11/index.json" --name "TC-FIAP-Grupo-11" --username $GITHUB_USERNAME --password $GITHUB_TOKEN --store-password-in-clear-text

# Copiar projetos do Users
COPY ["FCG.Api.Users/src/FCG.Api.Users/FCG.Api.Users.csproj", "FCG.Api.Users/src/FCG.Api.Users/"]
COPY ["FCG.Api.Users/src/FCG.Api.Users.Application/FCG.Api.Users.Application.csproj", "FCG.Api.Users/src/FCG.Api.Users.Application/"]
COPY ["FCG.Api.Users/src/FCG.Api.Users.Domain/FCG.Api.Users.Domain.csproj", "FCG.Api.Users/src/FCG.Api.Users.Domain/"]
COPY ["FCG.Api.Users/src/FCG.Api.Users.Infrastructure.AWS/FCG.Api.Users.Infrastructure.AWS.csproj", "FCG.Api.Users/src/FCG.Api.Users.Infrastructure.AWS/"]
COPY ["FCG.Api.Users/src/FCG.Api.Users.Infrastructure.Data/FCG.Api.Users.Infrastructure.Data.csproj", "FCG.Api.Users/src/FCG.Api.Users.Infrastructure.Data/"]

RUN dotnet restore "FCG.Api.Users/src/FCG.Api.Users/FCG.Api.Users.csproj"

COPY . .
WORKDIR "/src/FCG.Api.Users/src/FCG.Api.Users"
RUN dotnet build "FCG.Api.Users.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG.Api.Users.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.Api.Users.dll"]
