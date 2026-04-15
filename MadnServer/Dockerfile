# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MadnServer/MadnServer.csproj", "MadnServer/"]
COPY ["MadnShared/MadnShared.csproj", "MadnShared/"]

RUN dotnet restore "MadnServer/MadnServer.csproj"

COPY . .
WORKDIR /src/MadnServer
RUN dotnet publish "MadnServer.csproj" -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "MadnServer.dll"]

