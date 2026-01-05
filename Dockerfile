# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj from the subfolder
COPY EcommerceWeb.Api/EcommerceWeb.Api.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY EcommerceWeb.Api/. ./

# Publish
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Railway expects this port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EcommerceWeb.Api.dll"]
