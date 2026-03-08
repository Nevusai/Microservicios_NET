FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Gateway.API/Gateway.API.csproj", "Services/Gateway.API/"]
RUN dotnet restore "Services/Gateway.API/Gateway.API.csproj"

COPY . .
WORKDIR "/src/Services/Gateway.API"
RUN dotnet build "Gateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
