#!/bin/bash
# Script to manually apply database migrations

echo "Applying database migrations..."

dotnet ef database update

if [ $? -eq 0 ]; then
    echo "Migrations applied successfully!"
else
    echo "Error applying migrations."
    exit 1
fi
