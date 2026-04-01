# EventManager API V1
Small ASP.NET Core Web API - Events Manager. Education project - performs only CRUD operations.

## API
- GET /events — получить список событий
- GET /events/{id} — получить событие по id
- POST /events — создать событие
- PUT /events/{id} — обновить событие
- DELETE /events/{id} — удалить событие

## Run
```bash
cd EventManager
dotnet restore
dotnet build
dotnet run --launch-profile https
```

## Swagger
https://localhost:7113/swagger/index.html