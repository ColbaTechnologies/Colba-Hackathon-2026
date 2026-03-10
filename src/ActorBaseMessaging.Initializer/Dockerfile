FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/ActorBaseMessaging.Domain/ActorBaseMessaging.Domain.csproj             src/ActorBaseMessaging.Domain/
COPY src/ActorBaseMessaging.Infrastructure/ActorBaseMessaging.Infrastructure.csproj src/ActorBaseMessaging.Infrastructure/
COPY src/ActorBaseMessaging.Initializer/ActorBaseMessaging.Initializer.csproj   src/ActorBaseMessaging.Initializer/

RUN dotnet restore src/ActorBaseMessaging.Initializer/ActorBaseMessaging.Initializer.csproj --ignore-failed-sources

COPY src/ src/

RUN dotnet publish src/ActorBaseMessaging.Initializer/ActorBaseMessaging.Initializer.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .

RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "ActorBaseMessaging.Initializer.dll"]
