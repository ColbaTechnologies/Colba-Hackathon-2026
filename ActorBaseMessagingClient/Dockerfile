FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ActorBaseMessagingClient.csproj .

RUN dotnet restore ActorBaseMessagingClient.csproj --ignore-failed-sources

COPY . .

RUN dotnet publish ActorBaseMessagingClient.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "ActorBaseMessagingClient.dll"]
CMD ["http://api:8080"]
