# Pixel Service and Storage Service

This repository contains the Pixel Service and Storage Service, a service responsible for tracking and logging pixel requests in a file.

## How to Run

### Prerequisites

- Docker
- Docker Compose

### Build and Run with Docker

1. Navigate to the `Docker` directory:

    cd Docker

2. Run the following command to build and start the Pixel Service and the Storage Service along with other dependencies (e.g., Kafka, Zookeeper):

    docker-compose up --build

3. To test the Pixel Service and Storage Service, make a GET request to the following endpoint:

Endpoint: http://localhost:7071/track
This will simulate a pixel request, and you can check that a new log entry is added to the Storage Service's tmp/visitors.log file.

