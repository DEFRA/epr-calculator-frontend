FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EPR.Calculator.Frontend/NuGet.config", "EPR.Calculator.Frontend/"]
COPY ["EPR.Calculator.Frontend/EPR.Calculator.Frontend.csproj", "EPR.Calculator.Frontend/"]
COPY ["EPR.Calculator.Frontend.Common/EPR.Calculator.Frontend.Common.csproj", "EPR.Calculator.Frontend.Common/"]
RUN dotnet restore "./EPR.Calculator.Frontend/EPR.Calculator.Frontend.csproj"
COPY . .
WORKDIR "/src/EPR.Calculator.Frontend"
RUN dotnet build "./EPR.Calculator.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Calculator.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Calculator.Frontend.dll"]