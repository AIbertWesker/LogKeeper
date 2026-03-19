# LogKeeper (WIP)

Aplikacja do centralnego zbierania logów z innych aplikacji i zapisywania ich w bazie **NoSQL**. Repozytorium jest w trakcie rozwoju (**Work In Progress**). Projekt realizowany jako **zaliczenie na studiach**.

## Cel projektu

`LogKeeper` ma pełnić rolę prostego *log hub-a*:
- przyjmować logi wysyłane z innych aplikacji,
- zapisywać je w bazie NoSQL,
- udostępniać API do przeglądania i filtrowania logów,
- umożliwiać pobranie szczegółów pojedynczego wpisu (GET by ID).

## Zakres funkcjonalny (planowane)

### 1) Przyjmowanie logów (ingest)
- Wysyłanie logów do aplikacji pod dedykowany endpoint (HTTP).
- Docelowo wsparcie typowych metadanych, np. timestamp, poziom logu, źródło/aplikacja, środowisko, correlationId, message, payload/exception.

### 2) Pobieranie listy logów (z filtrowaniem)
Endpoint do pobierania listy logów oraz filtrowania po parametrach (przykładowo):
- zakres czasu (`from`, `to`),
- poziom (`level`),
- aplikacja/źródło (`source`),
- correlationId,
- paginacja i sortowanie.

### 3) Pobieranie szczegółów logu (GET by ID)
- Endpoint typu „get by id” zwracający kompletny rekord logu wraz ze szczegółami.

## API

Projekt jest budowany jako **Minimal API** (tzw. „mini API”) w .NET.

Dodatkowo dostępny jest prosty endpoint:
- `GET /health` — health check aplikacji.

## Status

- **WIP** — funkcjonalności i kontrakty API mogą się zmieniać.
- Celem jest iteracyjne dodawanie endpointów oraz warstwy persystencji (NoSQL) wraz z podstawową walidacją i obsługą błędów.

## Technologie (wstępnie)

- .NET (Minimal API)
- OpenAPI / Swagger
- Docelowo: baza NoSQL (do ustalenia/implementacji w trakcie)

## Uruchomienie (dev)

1. Sklonuj repozytorium
2. Uruchom projekt z Visual Studio lub przez CLI:
   - `dotnet run`
3. Swagger UI dostępny w trybie Development

## Licencja

Projekt edukacyjny (zaliczeniowy) — licencja MIT.
