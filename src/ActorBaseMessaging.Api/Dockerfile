FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/ActorBaseMessaging.Domain/ActorBaseMessaging.Domain.csproj             src/ActorBaseMessaging.Domain/
COPY src/ActorBaseMessaging.Application/ActorBaseMessaging.Application.csproj   src/ActorBaseMessaging.Application/
COPY src/ActorBaseMessaging.Infrastructure/ActorBaseMessaging.Infrastructure.csproj src/ActorBaseMessaging.Infrastructure/
COPY src/ActorBaseMessaging.Api/ActorBaseMessaging.Api.csproj                   src/ActorBaseMessaging.Api/

RUN dotnet restore src/ActorBaseMessaging.Api/ActorBaseMessaging.Api.csproj --ignore-failed-sources

COPY src/ src/

RUN dotnet publish src/ActorBaseMessaging.Api/ActorBaseMessaging.Api.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "ActorBaseMessaging.Api.dll"]
