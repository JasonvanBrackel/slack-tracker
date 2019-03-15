# Compile inside docker container
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

COPY ./ ./
RUN dotnet restore ./ChannelScraper/ChannelScraper.csproj
WORKDIR ./ChannelScraper
RUN dotnet publish --output /app/ --configuration Release

# Build Image
FROM microsoft/dotnet:2.2-runtime
WORKDIR /app
COPY --from=builder /app .

EXPOSE 3001

ENTRYPOINT dotnet ChannelScraper.dll
