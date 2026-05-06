# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs


FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestApp.Server/TestApp.Server.csproj", "TestApp.Server/"]
COPY ["testapp.client/testapp.client.esproj", "testapp.client/"]
COPY ["testapp.client/package.json", "testapp.client/"]
COPY ["testapp.client/package-lock.json", "testapp.client/"]

RUN dotnet restore "./TestApp.Server/TestApp.Server.csproj"
# Явно устанавливаем зависимости фронтенда внутри контейнера
WORKDIR "/src/testapp.client"
RUN npm install

# Возвращаемся и собираем всё вместе
WORKDIR "/src"
COPY . .
WORKDIR "/src/TestApp.Server"
RUN dotnet build "./TestApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TestApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestApp.Server.dll"]



## См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.
#
## 1. Этап установки Node.js и сборки фронтенда
#FROM node:20-alpine AS build-node
#WORKDIR /src/testapp.client
#ENV IS_DOCKER=true
#COPY ["testapp.client/package.json", "testapp.client/package-lock.json", "./"]
#RUN npm install
#COPY testapp.client/ .
#RUN npm run build
#
## 2. Этап сборки .NET
#FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["TestApp.Server/TestApp.Server.csproj", "TestApp.Server/"]
#RUN dotnet restore "./TestApp.Server/TestApp.Server.csproj"
#COPY . .
#RUN rm -rf testapp.client
#COPY --from=build-node /src/testapp.client ./TestApp.Server/wwwroot
#WORKDIR "/src/TestApp.Server"
#RUN dotnet build "./TestApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## 3. Публикация
#FROM build AS publish
#RUN dotnet publish "./TestApp.Server.csproj" /p:PublishSpaFiles=false -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -r linux-x64 --self-contained false
#
## 4. Финальный образ
#FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TestApp.Server.dll"]
#EXPOSE 8080
#EXPOSE 8081
#
#
