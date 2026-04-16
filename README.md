# EventManager API V1
Small ASP.NET Core Web API - Events Manager. Education project - performs only CRUD operations.

## API
- GET /events — получить список событий
- GET /events/{id} — получить событие по id
- POST /events — создать событие
- PUT /events/{id} — обновить событие
- DELETE /events/{id} — удалить событие

### GET /events - фильтрация и пагинация
Поддерживаемые query-параметры:
- `title` (string, optional) — фильтр по заголовку (регистр не важен, поиск по подстроке)
- `from` (date, optional, `yyyy-MM-dd`) — начальная дата (включительно)
- `to` (date, optional, `yyyy-MM-dd`) — конечная дата (включительно)
- `page` (int, optional, default: `1`) — номер страницы, начиная с 1
- `pageSize` (int, optional, default: `10`) — количество элементов на странице

Ограничения валидации:
- `page >= 1`
- `pageSize >= 1`

Примеры запросов:
- `GET /events?title=meetup`
- `GET /events?from=2026-01-01&to=2026-01-31`
- `GET /events?title=workshop&page=2&pageSize=5`

## Error response format
При ошибках API возвращает объект формата:

{
    ProblemDetails ErrorDetails,
    DateTime Created,
    string Message
}


Примеры ответов с ошибками:

`400 Bad Request` (некорректный `page`):
```json
{
  "errorDetails": {
    "title": "One or more validation errors occurred.",
    "status": 400,
    "detail": "$: JSON deserialization for type 'EventManager.Models.EventDto' was missing required properties including: 'title'., eventDto: The eventDto field is required."
  },
  "created": "2026-04-16T08:17:19.7257238Z",
  "message": "Validation issues. See Error Details"
}
```

`404 Not Found` (событие не найдено):
```json
{
  "errorDetails": {
    "status": 404,
    "detail": "Event 3fa85f64-5717-4562-b3fc-2c963f66afa6 is not found"
  },
  "created": "2026-04-16T08:23:09.1735054Z",
  "message": "Exception occured. See Error Details"
}
```

## Run
```bash
cd EventManager
dotnet restore
dotnet build
dotnet run --launch-profile https
```

## Run tests
```bash
cd EventManagerTests
dotnet test
```

## Swagger
https://localhost:7113/swagger/index.html