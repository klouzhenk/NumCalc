# Stage 1: build frontend (Vite)

FROM node:20-alpine AS frontend

WORKDIR /src/NumCalc.UI.Shared

COPY ./NumCalc.UI.Shared/package.json .
COPY ./NumCalc.UI.Shared/package-lock.json .
RUN npm ci

COPY ./NumCalc.UI.Shared .
RUN npm run build

# Stage 2: create the project itself

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ./NumCalc.Shared ./NumCalc.Shared/
COPY ./NumCalc.UI.Shared ./NumCalc.UI.Shared
COPY ./NumCalc.UI.Web ./NumCalc.UI.Web

COPY --from=frontend /src/NumCalc.UI.Shared/wwwroot/dist/ ./NumCalc.UI.Shared/wwwroot/dist/

RUN dotnet publish ./NumCalc.UI.Web/NumCalc.UI.Web.csproj -c Release -o /app/publish

# Stage 3: run app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NumCalc.UI.Web.dll"]