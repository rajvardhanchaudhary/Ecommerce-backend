# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj(s) first and restore dependencies (cached if unchanged)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Publish the app for Production
RUN dotnet publish -c Release -o /app/out /p:PublishTrimmed=true /p:PublishSingleFile=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out .

# Railway expects the app to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "EcommerceWeb.Api.dll"]
