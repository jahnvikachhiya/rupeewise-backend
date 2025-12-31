# Stage 1: Build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy only your API project file and restore
COPY ExpenseManagementAPI.csproj ./
RUN dotnet restore ExpenseManagementAPI.csproj

# Copy everything else and publish
COPY . ./
RUN dotnet publish ExpenseManagementAPI.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port Render will assign
ENV PORT 10000
EXPOSE $PORT

# Run the app
ENTRYPOINT ["dotnet", "ExpenseManagementAPI.dll"]
