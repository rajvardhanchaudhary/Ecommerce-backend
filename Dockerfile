# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore first (for better layer caching)
COPY EcommerceWeb.Api/EcommerceWeb.Api.csproj ./
RUN dotnet restore

# Copy source code
COPY EcommerceWeb.Api/. ./

# Publish (ensure code is null-safe!)
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user (security best practice)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published output
COPY --from=build /app/out .

# Railway / Render / most PaaS use port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EcommerceWeb.Api.dll"]
