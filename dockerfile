# Use the official image as a parent image.
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set the working directory.
WORKDIR /app

# Copy the file from your host to your current location.
COPY . .

# Run the command inside your image filesystem.
RUN dotnet restore

# Inform Docker that the container is listening on the specified port at runtime.
EXPOSE 3001

