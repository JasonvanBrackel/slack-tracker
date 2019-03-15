# Compile inside docker container
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

COPY ./ ./
RUN dotnet restore ./EventProcessor/EventProcessor.csproj
WORKDIR ./EventProcessor
RUN dotnet publish --output /app/ --configuration Release

# Build Image
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .

EXPOSE 8080

ENTRYPOINT dotnet EventProcessor.dll
