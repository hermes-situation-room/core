# Situation Room

An web based communication service for journalists and early stage activist groups created in the class ENG C183F, taken at UC Berkeley. Live at https://www.situation-room.org

## Install dependencies
(from inside `src/frontend/`)
```bash
yarn install
```

## Run the frontend locally
(from inside `src/frontend/`)
```bash
yarn start
```

## Build the frontend
(from inside `src/frontend/`)
```bash
yarn build
```

## 🐋 Docker
### Build Image
(from inside `src/frontend/`)
```bash
docker build -t com.hermes.situationroom.core.frontend .
```

### Run frontend locally in a docker container
(from inside `src/frontend/`)
```bash
docker run -p 8080:80 --name com.hermes.situationroom.core.frontend com.hermes.situationroom.core.frontend
```