#!/bin/sh

# Docker entrypoint script for Angular frontend
# This script allows runtime environment variable injection

# Default API URL if not provided
API_URL=${API_URL:-"http://localhost:5000"}

# Replace environment variables in the built files
echo "Configuring API URL to: $API_URL"
find /usr/share/nginx/html -name "*.js" -exec sed -i "s|__API_URL_PLACEHOLDER__|$API_URL|g" {} \;

GEMINI_API_KEY=${GEMINI_API_KEY:-""}
if [ -n "$GEMINI_API_KEY" ]; then
  find /usr/share/nginx/html -name "*.js" -exec sed -i "s|__GEMINI_API_KEY_PLACEHOLDER__|$GEMINI_API_KEY|g" {} \;
fi

# Execute the main command
exec "$@"