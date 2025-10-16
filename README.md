# 🌐 Hermes Core Repository

Welcome to the **Hermes Core Repository**!  
This repository contains the core components of the **Hermes Situation Room** project, including the backend, frontend, and Docker setup for both development and production environments.

---

## 📘 Table of Contents

1. [Project Overview](#project-overview)  
2. [Repository Structure](#repository-structure)  
3. [Development Setup](#development-setup)  
   - [Prerequisites](#prerequisites)  
   - [Backend Development](#backend-development)  
   - [Frontend Development](#frontend-development)  
4. [Full Local Docker For Stage Testing Purposes](#docker-setup)  
   - [Services](#services)  
   - [Running the Services](#running-the-services)  
   - [Pulling the Latest Images](#pulling-the-latest-images)  
   - [Stopping the Services](#stopping-the-services)  
5. [License](#license)

---

## 🛰️ Project Overview

**Hermes Situation Room** is a secure platform that **connects activists with journalists**, enabling the **anonymous**, **trusted**, and **safe exchange** of **sensitive information**.

It consists of:
- **Frontend** — a user-friendly web interface for interacting with the platform.  
- **Backend** — a robust API handling data processing, storage, and business logic.  
- **Database** — a Microsoft SQL Server instance for data persistence.

This repository contains all the necessary code and configurations to develop, test, and deploy the platform.

---

## 📂 Repository Structure

```
core/
├── LICENSE                     # License file
├── .github/workflows/          # CI/CD workflows for GitHub Actions
│   ├── docker-build-publish-backend.yml
│   ├── docker-build-publish-frontend.yml
│   └── quality-gate.yml
├── docs/                       # Documentation files
│   └── .gitkeep
├── src/                        # Source code
│   ├── backend/                # Backend services
│   │   ├── Hermes.SituationRoom.Data/
│   │   ├── Hermes.SituationRoom.Domain/
│   │   ├── Hermes.SituationRoom.Shared/
│   │   ├── Hermes.SituationRoom.Web/
│   │   └── Hermes.SituationRoom.Web.sln
│   └── frontend/               # Frontend services
└── compose.yaml                # Docker Compose configuration
```

---

# 🧠 Hermes Situation Room — Development Setup

Welcome to the **Hermes Situation Room** development guide.  
Follow these steps to get your environment up and running smoothly.

---

## 🧰 Prerequisites

Before starting, make sure you have the following installed:

1. **Docker** — Installed and running.  
2. **.NET SDK** — Required for backend development.  
3. **Node.js** — Required for frontend development.

---

## ⚙️ Backend Development

1. Navigate to the backend directory:
   ```bash
   cd src/backend
   ```

2. Open the solution file in your IDE:
   ```
   Hermes.SituationRoom.Web.sln
   ```

3. Run it locally in a Docker container:
   ```bash
   docker compose up --build -d
   ```

---

## 💻 Frontend Development

1. Navigate to the frontend directory:
   ```bash
   cd src/frontend
   ```

2. Install dependencies:
   ```bash
   yarn install
   ```

3. Start the frontend:
   ```bash
   yarn start
   ```

---

## 🐳 Full Local Docker For Stage Testing Purposes

The project uses **Docker** for both development and production environments.  
The Docker Compose configuration is located in:

```
delivery/compose.yaml
```

### 🧩 Services

| Service         | Description                            |
|-----------------|----------------------------------------|
| `frontend-prod` | Production-ready frontend service      |
| `api-prod`      | Production-ready backend service       |
| `mssql-prod`    | Microsoft SQL Server database          |

---

## 🚀 Running the Services

Navigate to the delivery directory:
```bash
cd delivery
docker compose up --build
```

Once running, access the services:

- **Frontend:** [http://localhost:13400](http://localhost:13400)  
- **Backend (Swagger):** [http://localhost:13500/swagger](http://localhost:13500/swagger)

---

## 🛑 Stopping the Services

To stop all running containers:
```bash
docker compose down
```

---

## 📄 License

This project is licensed under the terms of the **LICENSE** file.

For questions or issues, please open an issue in this repository.

---
✨ _Built with passion by the Hermes Team_
