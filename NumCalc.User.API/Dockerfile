# Stage 1: build application

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ./NumCalc.User.Domain ./NumCalc.User.Domain
COPY ./NumCalc.User.Application ./NumCalc.User.Application
COPY ./NumCalc.User.Infrastructure ./NumCalc.User.Infrastructure
COPY ./NumCalc.User.API ./NumCalc.User.API

RUN dotnet publish ./NumCalc.User.API/NumCalc.User.API.csproj -c Release -o /app/publish

# Stage 2: run application

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NumCalc.User.API.dll"]