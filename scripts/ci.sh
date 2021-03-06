#!/bin/sh

## Build Each Docker Image
docker build -t morehumansoftware/slack-user-scraper:latest -f ./scripts/UserScraper.Dockerfile ./src/Rancher.Community.Slack
docker build -t morehumansoftware/slack-channel-scraper:latest -f ./scripts/ChannelScraper.Dockerfile ./src/Rancher.Community.Slack
docker build -t morehumansoftware/slack-event-processor:latest -f ./scripts/EventProcessor.Dockerfile ./src/Rancher.Community.Slack
docker build -t morehumansoftware/slack-welcomer:latest -f ./scripts/Welcomer.Dockerfile ./src/Rancher.Community.Slack

## Push the Docker Images to hub.docker.com
docker push morehumansoftware/slack-user-scraper:latest
docker push morehumansoftware/slack-channel-scraper:latest
docker push morehumansoftware/slack-event-processor:latest
docker push morehumansoftware/slack-welcomer:latest