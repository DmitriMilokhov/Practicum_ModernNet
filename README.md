# EventManager API V1
Small ASP.NET Core Web API - Events Manager. Education project - performs only CRUD operations.

## API
- GET /events — получить список событий
- GET /events/{id} — получить событие по id
- POST /events — создать событие
- PUT /events/{id} — обновить событие
- DELETE /events/{id} — удалить событие
- POST /events/{id}/book — создать бронирование события (асинхронная обработка)
- GET /bookings/{id} — получить бронирование по id

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

### POST /events/{id}/book
Создает бронирование для события и помещает его в очередь фоновой обработки.

- Если событие не найдено: `404 Not Found`
- Если нет свободных мест на событии: `409 Conflict`
- Если бронирование принято в обработку: `202 Accepted`
- В ответе возвращается созданный `Booking` со статусом `Pending`

### GET /bookings/{id}
Возвращает текущее состояние бронирования.

- Если бронирование не найдено: `404 Not Found`
- При успехе: `200 OK`

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
## Event model
`Event` описывает событие и содержит:

- `id` (`Guid`) — идентификатор события
- `Title` (`string`) — заголовок события
- `Description` (`string?`) — описание события (не обязательно)
- `StartAt` (`DateTime`) — дата/время начала события (UTC)
- `EndAt` (`DateTime`) — время завершения события (UTC)
- `TotalSeats` (`int`) — общее количество мест для бронирования
- `AvailableSeats` (`int`) — количество свободных мест для бронирования

## Booking model
`Booking` описывает бронирование события и содержит:

- `id` (`Guid`) — идентификатор бронирования
- `eventId` (`Guid`) — идентификатор события
- `status` (`BookingStatus`) — текущий статус обработки
- `createdAt` (`DateTime`) — время создания бронирования (UTC)
- `processedAt` (`DateTime?`) — время завершения обработки (UTC), `null` пока не обработано

Статусы `BookingStatus`:

- `Pending` — заявка создана и ожидает обработки
- `Confirmed` — заявка успешно подтверждена фоновой обработкой
- `Rejected` — заявка отклонена 

## Логика фоновой обработки бронирований
После `POST /events/{id}/book` бронирование не подтверждается мгновенно.
API кладет заявку в in-memory очередь, затем фоновый воркер (`BookingBackgroundService`) обрабатывает ее:

1. Забирает следующий `Booking` из очереди
2. Имитирует асинхронную обработку (небольшая задержка)
3. Обновляет статус бронирования на `Confirmed`
4. Проставляет `processedAt`

Таким образом, `POST /events/{id}/book` возвращает быстрый `202 Accepted`, а итоговый статус нужно проверять через `GET /bookings/{id}`.

## Примитивы синхронизации
В проекте используются примитивы синхронизации, чтобы безопасно обрабатывать параллельные запросы на бронирование:

1. `Channel<T>` (`InMemoryTaskQueue`)  
   Используется как потокобезопасная очередь задач между API и фоновым обработчиком.
   - API добавляет `Booking` в очередь (`EnqueueAsync`)
   - `BookingBackgroundService` читает заявки через `ReadAllAsync`
   Это разделяет быстрый HTTP-ответ (`202 Accepted`) и более долгую обработку.

2. `SemaphoreSlim` + `ConcurrentDictionary<Guid, SemaphoreSlim>` (`EventBookingLockProvider`)  
   Используется для блокировки по `eventId`:
   - для одного и того же события одновременно выполняется только одна критическая операция (резерв/освобождение места)
   - для разных событий операции могут выполняться параллельно
   Это защищает от гонок и овербукинга при одновременных запросах.

3. `ConcurrentDictionary` в in-memory репозиториях  
   Обеспечивает потокобезопасный доступ к коллекциям сущностей (`Event`, `Booking`) при чтении/добавлении/удалении.

## Пример использования сценария бронирований
1. Создать событие:
   - `POST /events`
2. Получить `id` созданного события из ответа
3. Создать бронирование:
   - `POST /events/{id}/book`
4. Получить `bookingId` из ответа (`status = Pending`)
5. Через несколько секунд проверить статус:
   - `GET /bookings/{bookingId}`
6. Ожидаемый результат после обработки:
   - `status = Confirmed`, `processedAt != null`

## Пример сценария с овербукингом
Допустим, есть событие с `TotalSeats = 1`.

1. Клиент A отправляет `POST /events/{id}/book`  
   - получает `202 Accepted`, бронирование `A` со статусом `Pending`
2. Почти одновременно клиент B отправляет `POST /events/{id}/book`  
   - запрос выполняется конкурентно, но попадает под ту же блокировку по `eventId`
3. Первый запрос (A) резервирует единственное место  
4. Второй запрос (B), дождавшись блокировки, видит `AvailableSeats = 0`  
   - получает `409 Conflict` (`NoAvailableSeats`)
5. Воркер подтверждает заявку A, и через `GET /bookings/{A}` статус становится `Confirmed`

Итог: при высокой конкуренции подтверждается только допустимое число бронирований, а лишние запросы корректно отклоняются.

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