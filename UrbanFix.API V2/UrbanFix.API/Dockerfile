FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UrbanFix.API V2/UrbanFix.API/UrbanFix.API.csproj", "UrbanFix.API V2/UrbanFix.API/"]
RUN dotnet restore "UrbanFix.API V2/UrbanFix.API/UrbanFix.API.csproj"

COPY . .
RUN dotnet publish "UrbanFix.API V2/UrbanFix.API/UrbanFix.API.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "dotnet UrbanFix.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
