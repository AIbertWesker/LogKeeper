# LogKeeper (WIP)

Aplikacja do centralnego zbierania logów z innych aplikacji i zapisywania ich w bazie **NoSQL**. Repozytorium jest w trakcie rozwoju (**Work In Progress**). Projekt realizowany jako **zaliczenie na studiach**.

## Cel projektu

`LogKeeper` ma pełnić rolę prostego *log hub-a*:
- przyjmować logi wysyłane z innych aplikacji,
- zapisywać je w bazie NoSQL,
- udostępniać API do przeglądania i filtrowania logów,
- umożliwiać pobranie szczegółów pojedynczego wpisu (GET by ID).

## Zakres funkcjonalny

### 1) Przyjmowanie logów (ingest)
- Wysyłanie logów do aplikacji pod dedykowany endpoint (HTTP).
- Wsparcie typowych metadanych: timestamp, poziom, aplikacja, correlationId, message, payload/exception.

### 2) Pobieranie listy logów (z filtrowaniem)
Endpoint do pobierania listy logów oraz filtrowania po parametrach:
- zakres czasu (`from`, `to`),
- poziom (`level`),
- aplikacja/źródło (`application`),
- correlationId,
- paginacja cursor-based bez `Skip/Take`.

### 3) Pobieranie szczegółów logu (GET by ID)
- Endpoint typu „get by id” zwracający kompletny rekord logu wraz ze szczegółami.

## API

Projekt jest budowany jako **Minimal API** (tzw. „mini API”) w .NET.

Endpoints:
- `GET /health` — health check
- `GET /logs` — lista logów z filtrami i paginacją
- `GET /logs/{id}` — szczegóły logu
- `POST /logs` — zapis logu

## Status

- **WIP** — funkcjonalności i kontrakty API mogą się zmieniać.
- Celem jest iteracyjne dodawanie endpointów oraz warstwy persystencji (NoSQL) wraz z podstawową walidacją i obsługą błędów.

## Technologie

- .NET (Minimal API)
- OpenAPI / Swagger
- MongoDB (NoSQL)
- Serilog (producent logów)

## Uruchomienie (dev)

1. Sklonuj repozytorium
2. Uruchom API:
   - `dotnet run --project LogKeeper`
3. Uruchom frontend (statyczny hosting):
   - `dotnet run --project LogKeeper.Frontend`
4. (Opcjonalnie) uruchom producenta logów:
   - `dotnet run --project LogKeeper.LogProducer`
5. Swagger UI dostępny w trybie Development

Frontend domyślnie odwołuje się do `https://localhost:7222` — w razie potrzeby zmień to w UI.

## Licencja

Projekt edukacyjny (zaliczeniowy) — licencja MIT.
