# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["QuestionPapers.csproj", "./"]
RUN dotnet restore "QuestionPapers.csproj"

# Copy all files and build
COPY . .
RUN dotnet build "QuestionPapers.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "QuestionPapers.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Set port from environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
EXPOSE 5000

# Copy published files
COPY --from=build /app/publish .

# Entry point
ENTRYPOINT ["dotnet", "QuestionPapers.dll"]