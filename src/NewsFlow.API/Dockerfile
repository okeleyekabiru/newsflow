FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/NewsFlow.API/NewsFlow.API.csproj", "src/NewsFlow.API/"]
COPY ["src/NewsFlow.Core/NewsFlow.Core.csproj", "src/NewsFlow.Core/"]
COPY ["src/NewsFlow.Infrastructure/NewsFlow.Infrastructure.csproj", "src/NewsFlow.Infrastructure/"]
COPY ["src/NewsFlow.Shared/NewsFlow.Shared.csproj", "src/NewsFlow.Shared/"]

RUN dotnet restore "src/NewsFlow.API/NewsFlow.API.csproj"

COPY . .

WORKDIR "/src/src/NewsFlow.API"
RUN dotnet build "NewsFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewsFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewsFlow.API.dll"]
