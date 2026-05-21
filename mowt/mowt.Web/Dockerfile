# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["BilltrickV2/BilltrickV2.Web/BilltrickV2.Web.csproj", "BilltrickV2/BilltrickV2.Web/"]
COPY ["BilltrickV2/BilltrickV2.Web.Client/BilltrickV2.Web.Client.csproj", "BilltrickV2/BilltrickV2.Web.Client/"]
COPY ["BilltrickV2/BilltrickV2.Shared/BilltrickV2.Shared.csproj", "BilltrickV2/BilltrickV2.Shared/"]
COPY ["BilltrickV2.Shared.Models/BilltrickV2.Shared.Models.csproj", "BilltrickV2.Shared.Models/"]
RUN dotnet restore "./BilltrickV2/BilltrickV2.Web/BilltrickV2.Web.csproj"
COPY . .
WORKDIR "/src/BilltrickV2/BilltrickV2.Web"
RUN dotnet build "./BilltrickV2.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BilltrickV2.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BilltrickV2.Web.dll"]