# Use the official .NET SDK image as a build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory to /app
WORKDIR /app

# Copy the .csproj files to the working directory
COPY Application/Application.csproj PS/Application/Application.csproj
COPY Domain/Domain.csproj PS/Domain/Domain.csproj
COPY Infrastructure.CrossCutting/Infrastructure.CrossCutting.csproj PS/Infrastructure.CrossCutting/Infrastructure.CrossCutting.csproj
COPY Messaging.Events/Messaging.Events.csproj PS/Messaging.Events/Messaging.Events.csproj
COPY PixelService/PixelService.csproj PS/PixelService/PixelService.csproj
COPY PixelService.sln PS/PixelService.sln

COPY Tests.Unit/Tests.Unit.csproj PS/Tests.Unit/Tests.Unit.csproj

# Restore NuGet packages
RUN dotnet restore PS/PixelService.sln

# Copy the remaining source code to the working directory
COPY . .

# Build the application in Release mode
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory to /app
WORKDIR /app

# Copy the published application from the build image
COPY --from=build /app/out .

# Expose the port that your app will run on
EXPOSE 80

# Set the environment variables 
ENV ASPNETCORE_ENVIRONMENT Release

# Define the command to run your app
ENTRYPOINT ["dotnet", "PixelService.dll"]