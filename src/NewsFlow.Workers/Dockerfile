FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/NewsFlow.Workers/NewsFlow.Workers.csproj", "src/NewsFlow.Workers/"]
COPY ["src/NewsFlow.Core/NewsFlow.Core.csproj", "src/NewsFlow.Core/"]
COPY ["src/NewsFlow.Infrastructure/NewsFlow.Infrastructure.csproj", "src/NewsFlow.Infrastructure/"]
COPY ["src/NewsFlow.Shared/NewsFlow.Shared.csproj", "src/NewsFlow.Shared/"]

RUN dotnet restore "src/NewsFlow.Workers/NewsFlow.Workers.csproj"

COPY . .

WORKDIR "/src/src/NewsFlow.Workers"
RUN dotnet build "NewsFlow.Workers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewsFlow.Workers.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewsFlow.Workers.dll"]
