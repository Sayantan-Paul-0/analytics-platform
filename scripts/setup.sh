#!/bin/bash

echo "🚀 Setting up Analytics Platform..."

# Check prerequisites
command -v dotnet >/dev/null 2>&1 || { echo "❌ .NET 8 is not installed. Aborting."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "❌ Docker is not installed. Aborting."; exit 1; }

echo "✅ Prerequisites check passed"

# Build solution
echo "📦 Building solution..."
dotnet build

# Start infrastructure
echo "🐳 Starting Docker infrastructure..."
cd infrastructure/docker
docker-compose up -d

echo "⏳ Waiting for Kafka to be ready..."
sleep 15

echo "✅ Setup complete!"
echo ""
echo "Next steps:"
echo "  1. Run: ./scripts/run-services.sh"
echo "  2. Open http://localhost:8080 to view Kafka UI"
echo "  3. Send events to http://localhost:5000/api/events"