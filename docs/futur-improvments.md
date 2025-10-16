# Situation-Room - Now and Tasks for future improvements

> This Document contains tasks and suggestions for future improvements and additional features. There are some technical details that are included in this document for deeper understanding of the application.

- [Situation-Room - Now and Tasks for future improvements](#situation-room---now-and-tasks-for-future-improvements)
  - [Future tasks and suggestions](#future-tasks-and-suggestions)
    - [Journalist E-Mail verification](#journalist-e-mail-verification)
    - [Extend journalist profile to help build trust for activists](#extend-journalist-profile-to-help-build-trust-for-activists)
    - [Extend Real-Time Updates in the application](#extend-real-time-updates-in-the-application)
    - [End-to-end encryption for real-time messaging service](#end-to-end-encryption-for-real-time-messaging-service)
    - [Add trust-score for activist to rate his liability](#add-trust-score-for-activist-to-rate-his-liability)
  - [Technical insights](#technical-insights)
    - [Used technology](#used-technology)
      - [Frontend](#frontend)
      - [Backend](#backend)
      - [Database](#database)
    - [Application setup](#application-setup)

## Future tasks and suggestions

### Journalist E-Mail verification

Journalist should be required to verify their E-Mail-Address. This prevents spam and untrustworthy journalist-profiles.

The E-Mail-Verification can go even further and checks if the E-Mail-Address is associated with a known newspaper or news agency.

### Extend journalist profile to help build trust for activists

Journalists should have the possibility to extend their profile with additional information such as their professional social media account, links to past news-articles or a short bio.

This additions help activists build trust in journalists and can check if they want to share the information with them.

### Extend Real-Time Updates in the application

At the moment, real-time updates are provided for our messaging service. The real-time capability can or should be extended all over the application to improve user experience. This would include new posts, updates on posts, new chats and new comments.

> _**Technical hint**: Real-time messaging is implemented using `websockets` and the library `signalR` provided by Microsoft_

### End-to-end encryption for real-time messaging service

Implement end-to-end encryption to secure all messages. The system must consider that users might be logged in on multiple devices. As a second step, extend the functionality to allow users to directly verify their contact's encryption key (for example, by scanning a QR code). This will ensure the connection is secure and prevent any "man-in-the-middle" attacks.

### Add trust-score for activist to rate his liability

A key feature is that activists can remain anonymous, but this makes it hard for journalists to evaluate if an activist is a reliable source. To solve this, journalists should have the ability to "vouch for" an activist after a positive interaction. Each vouch from a journalist will contribute to a visible trust score on the activist's profile, helping other journalists identify credible sources without forcing the activist to reveal their personal information.

## Technical insights

### Used technology

The Situation-Room application is splitted into three main parts:

- Frontend (the view of the user)
- Backend (Implemented Business-logic)
- Database (Data-storage)

#### Frontend

The frontend is build using the [Vue.js](https://vuejs.org) JavaScript library. Vue.js is configured to use Typescript for logical operations and Vuejs-Markup for defining the user-interface.

#### Backend

The backend holding the business-logic is written in .NET C#. The backend is designed to be a WebApi and it's endpoints are exposed to the internet.

It uses Entity Framework Core for Database queries and provide the functionality for real-time messaging over `signalR`-WebSockets.

#### Database

The Database is considered to be a Microsoft SQL Server Database, encapsulated in a Docker-Container.

### Application setup

All of the three parts of our application are encapsulated as docker containers.

In addition the the previous mentioned containers are two more:

- nginx reverse proxy
- CertBot for SSL certificates

All these containers are hosted on a virtual server (Ubuntu Server OS) in a german data-center.
