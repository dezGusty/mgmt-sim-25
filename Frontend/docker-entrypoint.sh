#!/bin/sh

# Docker entrypoint script for Angular frontend
# This script allows runtime environment variable injection

# Default API URL if not provided
API_URL=${API_URL:-"http://localhost:5000"}

# Replace environment variables in the built files
if [ -f /usr/share/nginx/html/main*.js ]; then
    echo "Configuring API URL to: $API_URL"
    # Find and replace placeholder in built JavaScript files
    find /usr/share/nginx/html -name "*.js" -exec sed -i "s|__API_URL_PLACEHOLDER__|$API_URL|g" {} \;
fi

# Execute the main command
exec "$@"