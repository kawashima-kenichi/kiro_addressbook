# Backend Dockerfile - ASP.NET Core 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files for restore
COPY AddressBook.slnx ./
COPY src/AddressBook.API/AddressBook.API.csproj src/AddressBook.API/
COPY src/AddressBook.Application/AddressBook.Application.csproj src/AddressBook.Application/
COPY src/AddressBook.Domain/AddressBook.Domain.csproj src/AddressBook.Domain/
COPY src/AddressBook.Infrastructure/AddressBook.Infrastructure.csproj src/AddressBook.Infrastructure/

# Restore dependencies
RUN dotnet restore AddressBook.slnx

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish src/AddressBook.API/AddressBook.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS runtime
WORKDIR /app

# Create non-root user for security
RUN useradd -m -s /bin/bash appuser || adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "AddressBook.API.dll"]
