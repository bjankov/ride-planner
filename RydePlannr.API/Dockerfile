FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RydePlannr.API/RydePlannr.API.csproj", "RydePlannr.API/"]
COPY ["RydePlannr.Application/RydePlannr.Application.csproj", "RydePlannr.Application/"]
COPY ["RydePlannr.Domain/RydePlannr.Domain.csproj", "RydePlannr.Domain/"]
COPY ["RydePlannr.Infrastructure/RydePlannr.Infrastructure.csproj", "RydePlannr.Infrastructure/"]
RUN dotnet restore "RydePlannr.API/RydePlannr.API.csproj"
COPY . .
WORKDIR "/src/RydePlannr.API"
RUN dotnet build "./RydePlannr.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RydePlannr.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RydePlannr.API.dll"]
