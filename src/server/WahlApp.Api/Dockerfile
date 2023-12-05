#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
VOLUME /app/data

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["server/WahlApp.Api/WahlApp.Api.csproj", "WahlApp.Api/"]
RUN dotnet restore "WahlApp.Api/WahlApp.Api.csproj"
COPY . .
WORKDIR "/src/server/WahlApp.Api"
RUN dotnet build "WahlApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WahlApp.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WahlApp.Api.dll"]
