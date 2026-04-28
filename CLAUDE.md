# GlucoKids Backend — Agent Context

## Проєкт
ASP.NET Core 8 Minimal API — проксі між Android-клієнтом та FatSecret Platform API.
Частина дитячого освітнього застосунку про діабет.

---

## Структура

```
GlucoKidsBackend/GlucoKids/
├── docker-compose.yml
├── global.json
├── GlucoKids.sln
└── GlucoKids/
    ├── Program.cs                        ← єдина точка входу, всі маршрути
    ├── GlucoKids.csproj                  ← net8.0, Swashbuckle
    ├── Dockerfile                        ← Linux контейнер
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Models/
    │   └── FoodModels.cs                 ← FoodItem, FoodSearchResponse (records)
    └── Services/
        ├── FatSecretTokenService.cs      ← OAuth 2.0 Client Credentials, кеш 24г
        └── FatSecretFoodService.cs       ← foods.search.v3, парсинг нутрієнтів
```

---

## Tech Stack
- **.NET 8** — ASP.NET Core Minimal API
- **Swashbuckle / Swagger** — OpenAPI документація
- **Docker** — Linux контейнер
- **FatSecret Platform API** — пошук їжі та нутрієнти

---

## API

### `GET /api/food/search?q={query}&page={page}`
Пошук їжі через FatSecret. Повертає `FoodSearchResponse`.

**Відповідь:**
```json
{
  "foods": [
    {
      "name": "Гречка",
      "calories": 343,
      "carbs": 71.5,
      "breadUnits": 5.96,
      "brand": null
    }
  ],
  "total": 42
}
```

**Хлібна одиниця:** `carbs / 12` (12г вуглеводів = 1 ХО).

---

## FatSecret інтеграція

### Auth flow
1. `FatSecretTokenService.GetAccessTokenAsync()` — перевіряє кеш
2. Якщо токен протермінований — POST на `https://oauth.fatsecret.com/connect/token`
3. OAuth 2.0 Client Credentials, scope `basic localization`
4. Токен кешується в пам'яті на `expires_in - 60` секунд
5. Обидва сервіси — `Singleton` (токен живе разом з додатком)

### Search flow
- Метод: `foods.search.v3`
- Параметри: `language=uk`, `region=UA`, `max_results=20`
- Хіт: `POST https://platform.fatsecret.com/rest/server.api`
- Парсинг: `servings.serving[0]` → `calories`, `carbohydrate`; fallback — `food_description` рядок

### Secrets
- `FatSecret:ClientId` і `FatSecret:ClientSecret`
- **Локально:** .NET User Secrets (`dotnet user-secrets`)
- **Docker:** `.env` файл (не комітити!)
- **Ніколи** не захардкоджувати в коді

---

## Моделі

```csharp
public record FoodItem(string Name, int Calories, float Carbs, float BreadUnits, string? Brand);
public record FoodSearchResponse(List<FoodItem> Foods, int Total);
```

---

## Що наступне (пріоритети)

1. **FatSecret Premier** — подати заявку (студент, безкоштовно) → UA датасет (region=UA)
2. **CORS** — дозволити запити з Android емулятора (`10.0.2.2`) та реального пристрою
3. **Rate limiting** — захист від зайвих запитів до FatSecret
4. **Endpoint розширення** — деталі продукту `GET /api/food/{id}`, логування глюкози
5. **Firebase Admin SDK** — читання/запис записів глюкози, прогресу уроків
6. **Auth middleware** — перевірка Firebase ID Token у заголовку запиту
7. **Health check** — `GET /health` для Docker

---

## Запуск локально

```bash
# User Secrets (один раз)
dotnet user-secrets set "FatSecret:ClientId" "<your-id>"
dotnet user-secrets set "FatSecret:ClientSecret" "<your-secret>"

# Запуск
dotnet run --project GlucoKids/GlucoKids.csproj

# Swagger UI
https://localhost:7xxx/swagger
```

## Запуск в Docker

```bash
docker-compose up --build
```

---

## Відомі обмеження

- `FatSecretTokenService` — не thread-safe при одночасному першому запиті (race condition на холодному старті); для прод — `SemaphoreSlim`
- UA датасет (region=UA) вимагає FatSecret Premier tier
- Немає логування запитів (ILogger не підключений)
- Немає юніт-тестів
