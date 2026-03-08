FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Orders.API/Orders.API.csproj", "Services/Orders.API/"]
COPY ["Shared/SharedModels/SharedModels.csproj", "Shared/SharedModels/"]
RUN dotnet restore "Services/Orders.API/Orders.API.csproj"

COPY . .
WORKDIR "/src/Services/Orders.API"
RUN dotnet build "Orders.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Orders.API.dll"]
