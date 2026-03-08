FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Users.API/Users.API.csproj", "Services/Users.API/"]
COPY ["Shared/SharedModels/SharedModels.csproj", "Shared/SharedModels/"]
RUN dotnet restore "Services/Users.API/Users.API.csproj"

COPY . .
WORKDIR "/src/Services/Users.API"
RUN dotnet build "Users.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Users.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Users.API.dll"]
