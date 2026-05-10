FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ["GlucoKids/GlucoKids.csproj",                         "GlucoKids/"]
COPY ["GlucoKids.Application/GlucoKids.Application.csproj", "GlucoKids.Application/"]
COPY ["GlucoKids.Domain/GlucoKids.Domain.csproj",           "GlucoKids.Domain/"]
COPY ["GlucoKids.Infrastructure/GlucoKids.Infrastructure.csproj", "GlucoKids.Infrastructure/"]

RUN dotnet restore "GlucoKids/GlucoKids.csproj"

COPY . .

RUN dotnet publish "GlucoKids/GlucoKids.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GlucoKids.dll"]
