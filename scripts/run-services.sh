#!/bin/bash

echo "🚀 Starting all services..."

# Start EventProducer
echo "Starting EventProducer on port 5001..."
dotnet run --project src/Services/EventProducer/EventProducer/EventProducer.csproj --urls "http://localhost:5001" &
PRODUCER_PID=$!

# Start EventConsumer
echo "Starting EventConsumer on port 5002..."
dotnet run --project src/Services/EventConsumer/EventConsumer/EventConsumer.csproj --urls "http://localhost:5002" &
CONSUMER_PID=$!

# Start ApiGateway
echo "Starting ApiGateway on port 5000..."
dotnet run --project src/Services/ApiGateway/ApiGateway/ApiGateway.csproj --urls "http://localhost:5000" &
GATEWAY_PID=$!

echo ""
echo "✅ All services started!"
echo ""
echo "Services running:"
echo "  - API Gateway: http://localhost:5000"
echo "  - Event Producer: http://localhost:5001"
echo "  - Event Consumer: http://localhost:5002"
echo "  - Kafka UI: http://localhost:8080"
echo ""
echo "To stop services, press Ctrl+C"

# Wait for all background processes
wait $PRODUCER_PID $CONSUMER_PID $GATEWAY_PID