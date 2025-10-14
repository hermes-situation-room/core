# 🖥️ HERMES SITUATION ROOM — FRONTEND

The **Situation Room Frontend** is a secure, web-based interface that connects activists and journalists through the Hermes ecosystem.  
It provides the user-facing layer for encrypted communication, tip-sharing, and verification workflows.

---

## ⚙️ Tech Stack

- **Framework:** Vue.js  
- **Package Manager:** Yarn  
- **Build System:** Vite
- **Deployment:** Dockerized and served via Nginx in release stage  
- **Environment:** Production build served at `hermes-frontend-release` (internal, proxied by Nginx)

---

## 📦 Project Setup

### Install Dependencies
```bash
yarn install
```

### Run Locally (Development Mode)
```bash
yarn start
```

### Build for Production
```bash
yarn build
```

The production build output is placed in the `dist/` directory and used by the **frontend Docker image**.

---

## 🐋 Docker Usage

### Build Docker Image
```bash
docker build -t hermes-situationroom-frontend .
```

### Run Locally in Docker
```bash
docker run -p 8080:80 --name hermes-situationroom-frontend hermes-situationroom-frontend
```

The app will be accessible at **http://localhost:8080**.

---

## 🔗 Release Integration

In the **release (production)** environment:
- The container runs as `hermes-frontend-release` on the internal Docker network.
- It is served **only through the Nginx reverse proxy** (`nginx-release-proxy`).
- External users never access the container directly — all requests go through HTTPS via Nginx.

---

## 🚀 Quick Commands

| Action                 | Command                                                  |
|-------------------------|--------------------------------                         |
| Install dependencies    | `yarn install`                                          |
| Start dev server        | `yarn start`                                            |
| Build production files  | `yarn build`                                            |
| Build Docker image      | `docker build -t hermes-situationroom-frontend .`       |
| Run Docker container    | `docker run -p 8080:80 hermes-situationroom-frontend`   |
