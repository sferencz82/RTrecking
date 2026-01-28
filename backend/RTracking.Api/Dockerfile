FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RTracking.Api.csproj", "./"]
RUN dotnet restore "RTracking.Api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RTracking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RTracking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef --version 8.0.0 || true
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create directories for uploads and tessdata
RUN mkdir -p /app/wwwroot/uploads/receipts
RUN mkdir -p /app/tessdata

ENTRYPOINT ["dotnet", "RTracking.Api.dll"]
