# Slack Tracker

This repository is a suite of utilites for interacting and collecting information on Slack Workspaces.

## Feature Projects

### Channel Scraper

The Channel Scaper grabs available public channel and message history and stores it in a SQL Server database.

** Channel Metadata **
For each channel this will grab
- Channel Name
- Message History

#### Settings


**slack_url** - The api url for the slack api
**authorization_token** - The authorization token for the slack user who will scrape channels
**db_connection** - Connection to the database where users metadata is stored.

### User Scraper

The User Scraper grabs Slack user and stores it in a SQL Server database.

** User Metadata **
For each user this will grab
- Slack Id
- Username (this feature has been largely deprecated from Slack, but still sent from the API)
- Name (the display name of the user)
- Email Address
- Timezone
- Timezone Label
- Image Path 

#### Settings

**slack_url** - The api url for the slack api
**authorization_token** - The authorization token for the slack user who will scrape users
**db_connection** - Connection to the database where users metadata is stored.

### Welcomer

The Welcomer regularly sends a welcome message from a Slack user to new and existing Slack members.  Each are set by setting the appropriate environment variable

#### Welcomer Settings 

**welcome_message_new_users** - Set markdown text to send a message to new users.
**welcome_message_existing_users** - Set markdown text to send a message to exsiting users.
**welcomer_id** - Slack id of the user sending welome messages.  Used to make sure the user is online when messages are sent.

#### Other Settings

**slack_url** - The api url for the slack api
**authorization_token** - The authorization token for the slack user who will send the messages.
**db_connection** - Connection to the database where users and welcome metadata is stored.

### Event Processor

This Event Processor grabs live events from the Slack Workspace.  At this time it handles the following events

- Channel Created Events
- Member Joined Channel Events
- Message Sent Events

It uses this to track new users who have yet to be welcomed to the slack channel.