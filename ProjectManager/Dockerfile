FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ProjectManager/ProjectManager.csproj", "ProjectManager/"]
RUN dotnet restore "ProjectManager/ProjectManager.csproj"

COPY . .
WORKDIR "/src/ProjectManager"
RUN dotnet publish "ProjectManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProjectManager.dll"]
