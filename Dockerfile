ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Opengate/Opengate.csproj", "src/Opengate/"]
RUN dotnet restore "./src/Opengate/Opengate.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./src/Opengate/Opengate.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Opengate/Opengate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Opengate.dll"]
