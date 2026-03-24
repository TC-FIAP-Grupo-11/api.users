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

# Install New Relic .NET agent
RUN apt-get update \
    && apt-get install -y wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget https://download.newrelic.com/548C16BF.gpg \
    && apt-key add 548C16BF.gpg \
    && apt-get update \
    && apt-get install -y newrelic-dotnet-agent \
    && rm -rf /var/lib/apt/lists/*

ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
    NEW_RELIC_HOME=/usr/local/newrelic-dotnet-agent

ENTRYPOINT ["dotnet", "FCG.Api.Users.dll"]
