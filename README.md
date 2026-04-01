# EventManager API V1
Small ASP.NET Core Web API - Events Manager. Education project - performs only CRUD operations.

## API
- GET /api/events — получить список событий
- GET /api/events/{id} — получить событие по id
- POST /api/events — создать событие
- PUT /api/events/{id} — обновить событие
- DELETE /api/events/{id} — удалить событие

## RUN
```bash
dotnet build
dotnet run --launch-profile https