# CurrencyConverter.API

A .NET 10 ASP.NET Core API for currency conversion with unit tests and Docker support.

## Prerequisites

- Docker Desktop
- Git

## Clone the Project

```powershell
git clone https://github.com/1999rahul/CurrencyConverter.API.git
cd CurrencyConverter.API
```

Run the following commands from the project root (the folder that contains `docker-compose.yml`).

## Run with Docker

```powershell
# Build and run from scratch (no cache)
docker-compose down
docker-compose build --no-cache
docker-compose up api
```

API will be available at `http://localhost:8080/Convert?SourceCurrency=USD&TargetCurrency=INR&Amount=10`

Tests run automatically during Docker build and output to console.

## Stop Docker Container

```powershell
docker-compose down
```
