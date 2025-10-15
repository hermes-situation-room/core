# Stage 1: Build the application
FROM node:20-alpine AS build

# Set working directory inside container
WORKDIR /app

# Copy package files first for better caching
COPY package.json yarn.lock ./

# Install dependencies with Yarn
RUN yarn install

# Copy the rest of the application code
COPY . .

# Build the Vue.js application
RUN yarn build


# Stage 2: Serve with a lightweight web server
FROM nginx:alpine

# Copy built files from previous stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx config
# e.g., handles Vue router history mode
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx server
CMD ["nginx", "-g", "daemon off;"]
