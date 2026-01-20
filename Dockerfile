FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar projetos do Users
COPY ["src/FCG.Api.Users/FCG.Api.Users.csproj", "src/FCG.Api.Users/"]
COPY ["src/FCG.Api.Users.Application/FCG.Api.Users.Application.csproj", "src/FCG.Api.Users.Application/"]
COPY ["src/FCG.Api.Users.Domain/FCG.Api.Users.Domain.csproj", "src/FCG.Api.Users.Domain/"]
COPY ["src/FCG.Api.Users.Infrastructure.AWS/FCG.Api.Users.Infrastructure.AWS.csproj", "src/FCG.Api.Users.Infrastructure.AWS/"]
COPY ["src/FCG.Api.Users.Infrastructure.Data/FCG.Api.Users.Infrastructure.Data.csproj", "src/FCG.Api.Users.Infrastructure.Data/"]

RUN dotnet restore "src/FCG.Api.Users/FCG.Api.Users.csproj"

COPY . .
WORKDIR "/src/src/FCG.Api.Users"
RUN dotnet build "FCG.Api.Users.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG.Api.Users.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.Api.Users.dll"]
