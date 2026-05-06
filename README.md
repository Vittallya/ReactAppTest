# TestApp (Fullstack .NET 10 + React)

Это приложение состоит из backend на ASP.NET Core и frontend на React (Vite).
Сборка и запуск полностью контейнеризированы.

## Требования
*   [Docker Desktop](https://docker.com) (или Docker Engine в Linux)

## Быстрый запуск (Docker)

### 1. Сборка образа
Запустите команду из корневой директории проекта (там, где лежит Dockerfile):
```bash
docker build -t test-app .
```

### 2. Запуск контейнера
```bash
docker run -d -p 8080:8080 --name test-app-container test-app
```

Приложение будет доступно по адресу: **http://localhost:8080**
