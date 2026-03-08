FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Products.API/Products.API.csproj", "Services/Products.API/"]
COPY ["Shared/SharedModels/SharedModels.csproj", "Shared/SharedModels/"]
RUN dotnet restore "Services/Products.API/Products.API.csproj"

COPY . .
WORKDIR "/src/Services/Products.API"
RUN dotnet build "Products.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Products.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Products.API.dll"]
