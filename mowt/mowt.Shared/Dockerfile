FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5140

ENV ASPNETCORE_URLS=http://+:5140

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["mowt/mowt.Shared/mowt.Shared.csproj", "mowt/mowt.Shared/"]
COPY ["mowt/mowt/mowt.csproj", "mowt/mowt"]
RUN dotnet restore "mowt/mowt.Shared/mowt.Shared.csproj"
RUN dotnet restore "mowt/mowt.csproj"
COPY . .
WORKDIR "/src/mowt"
RUN dotnet build "mowt/mowt.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "mowt/mowt.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mowt.dll"]
