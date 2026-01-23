# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

# Update package lists and install prerequisites
RUN apt-get update && \
    apt-get install -y curl gnupg

# Add NodeSource repository
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash -

# Install Node.js
RUN apt-get install -y nodejs

# Optional: Verify installation
RUN node --version
RUN npm --version

WORKDIR /app
COPY ["TheGatekeeper.Server/TheGateKeeper.Server.csproj", "TheGatekeeper.Server/"]
COPY ["TheGatekeeper.Contracts/TheGatekeeper.Contracts.csproj", "TheGatekeeper.Contracts/"]
RUN dotnet restore "TheGatekeeper.Server/TheGateKeeper.Server.csproj"
COPY . .
WORKDIR "/app/."
RUN dotnet tool install -g dotnet-typegen
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet build "TheGatekeeper.Contracts/TheGatekeeper.Contracts.csproj" -c $BUILD_CONFIGURATION
RUN dotnet-typegen generate -p "TheGatekeeper.Contracts"

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TheGatekeeper.Server/TheGateKeeper.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheGateKeeper.Server.dll"]