FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RydePlannr.AuthService/RydePlannr.AuthService.csproj", "RydePlannr.AuthService/"]
COPY ["RydePlannr.Domain/RydePlannr.Domain.csproj", "RydePlannr.Domain/"]
COPY ["RydePlannr.Infrastructure/RydePlannr.Infrastructure.csproj", "RydePlannr.Infrastructure/"]
RUN dotnet restore "RydePlannr.AuthService/RydePlannr.AuthService.csproj"
COPY . .
WORKDIR "/src/RydePlannr.AuthService"
RUN dotnet build "./RydePlannr.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RydePlannr.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RydePlannr.AuthService.dll"]
