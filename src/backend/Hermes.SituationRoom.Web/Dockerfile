FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hermes.SituationRoom.Web/Hermes.SituationRoom.Api.csproj", "Hermes.SituationRoom.Web/"]
COPY ["Hermes.SituationRoom.Data/Hermes.SituationRoom.Data.csproj", "Hermes.SituationRoom.Data/"]
RUN dotnet restore "Hermes.SituationRoom.Web/Hermes.SituationRoom.Api.csproj"
COPY . .
WORKDIR "/src/Hermes.SituationRoom.Web"
RUN dotnet build "Hermes.SituationRoom.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Hermes.SituationRoom.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hermes.SituationRoom.Api.dll"]
