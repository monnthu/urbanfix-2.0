FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Urbanfix/Urbanfix.csproj Urbanfix/
RUN dotnet restore Urbanfix/Urbanfix.csproj
COPY Urbanfix/ Urbanfix/
RUN dotnet publish Urbanfix/Urbanfix.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "Urbanfix.dll"]
