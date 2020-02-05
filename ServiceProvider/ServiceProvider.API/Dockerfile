FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

RUN apt-get update && apt-get install -y --no-install-recommends dos2unix

WORKDIR /src/ServiceProvider.API

# copy csproj and restore as distinct layers
COPY ServiceProvider.API/*.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY ServiceProvider.API/. .
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app .