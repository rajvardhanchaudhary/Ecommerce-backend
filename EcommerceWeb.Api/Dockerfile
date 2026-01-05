# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Publish the app
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Set the port Railway uses
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "EcommerceWeb.Api.dll"]
