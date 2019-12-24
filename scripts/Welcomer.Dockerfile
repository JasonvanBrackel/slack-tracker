# Compile inside docker container
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

COPY ./ ./
RUN dotnet restore ./Welcomer/Welcomer.csproj
WORKDIR ./Welcomer
RUN dotnet publish --output /app/ --configuration Release

# Build Image
FROM microsoft/dotnet:3.1-runtime
WORKDIR /app
COPY --from=builder /app .

EXPOSE 3001

ENTRYPOINT dotnet Welcomer.dll
