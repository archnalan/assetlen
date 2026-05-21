# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["mowt.API/mowt.API.csproj", "mowt.API/"]
COPY ["mowt.Service/mowt.Service.csproj", "mowt.Service/"]
COPY ["mowt.Shared.Models/mowt.Shared.Models.csproj", "mowt.Shared.Models/"]
COPY ["mowt/mowt.Shared/mowt.Shared.csproj", "mowt/mowt.Shared/"]
COPY ["mowt.Sqlite/mowt.Sqlite.csproj", "mowt.Sqlite/"]
COPY ["mowt.SqlServer/mowt.SqlServer.csproj", "mowt.SqlServer/"]
RUN dotnet restore "./mowt.API/mowt.API.csproj"
COPY . .
WORKDIR "/src/mowt.API"
RUN dotnet build "./mowt.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./mowt.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mowt.API.dll"]