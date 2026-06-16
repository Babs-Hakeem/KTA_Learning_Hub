# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["KTALearningHub.API.csproj", "./"]
RUN dotnet restore "KTALEARNINGHUB.csproj"

# Copy the rest of the project
COPY . .
WORKDIR "/src"

# Build the project
RUN dotnet build "KTALEARNINGHUB.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "KTALEARNINGHUB.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KTALEARNINGHUB.dll"]
