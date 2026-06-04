# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CurrencyConverter.API/CurrencyConverter.API.csproj", "CurrencyConverter.API/"]
COPY ["tests/CurrencyConverterApi.Tests/CurrencyConverterApi.Tests.csproj", "tests/CurrencyConverterApi.Tests/"]
RUN dotnet restore "./CurrencyConverter.API/CurrencyConverter.API.csproj"
RUN dotnet restore "./tests/CurrencyConverterApi.Tests/CurrencyConverterApi.Tests.csproj"
COPY . .
WORKDIR "/src/CurrencyConverter.API"
RUN dotnet build "./CurrencyConverter.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Run tests
WORKDIR "/src/tests/CurrencyConverterApi.Tests"
RUN dotnet test -c Release

# Publish the service project
WORKDIR "/src/CurrencyConverter.API"
RUN dotnet publish "./CurrencyConverter.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - copy published app and run
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CurrencyConverter.API.dll"]