FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ShopAPI.csproj","./"]
RUN dotnet restore "./ShopAPI.csproj"

COPY . .
RUN dotnet publish "./ShopAPI.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

From mcr.microsoft.com/dotnet/aspnet:10.0 AS final 
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet","ShopAPI.dll"]