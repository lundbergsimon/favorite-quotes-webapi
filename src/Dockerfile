FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY favorite-quotes-webapi.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Publish the application to the /app/publish directory
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official ASP.NET runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS prod
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 5173 for HTTP traffic
EXPOSE 5173

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "favorite-quotes-webapi.dll"]