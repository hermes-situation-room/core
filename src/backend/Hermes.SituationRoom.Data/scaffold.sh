#!/bin/bash

# Exit immediately if a command fails
set -e

# Run EF Core scaffold using the connection name from appsettings.json
dotnet ef dbcontext scaffold "Server=127.0.0.1,1434;User Id=sa;Password=Docker.SituationRoom.2024;TrustServerCertificate=True;Initial Catalog=HERMESSituationRoom;" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir ./Entities \
  --context-dir ./Context \
  --table "dbo.Activist" \
  --table "dbo.Chat" \
  --table "dbo.Comment" \
  --table "dbo.Journalist" \
  --table "dbo.Message" \
  --table "dbo.Post" \
  --table "dbo.PrivacyLevelPersonal" \
  --table "dbo.User" \
  --table "dbo.PostTag" \
  --no-onconfiguring \
  -f

