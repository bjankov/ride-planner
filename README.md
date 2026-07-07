# RydePlannr

REST API za planiranje biciklističkih vožnji i događaja, podijeljen u dva servisa:

- **RydePlannr.API** — klubovi, rute, lokacije, događaji vožnji, poruke
- **RydePlannr.AuthService** — registracija i prijava (izdaje JWT tokene koje API validira)

Oba servisa dijele jednu PostgreSQL bazu podataka.

## Pokretanje svega (Docker)

Potreban je pokrenut Docker Desktop.

```bash
docker compose up -d --build
```

Ovo pokreće tri kontejnera: `db` (Postgres), `authservice` i `api`. API pri pokretanju automatski primjenjuje EF Core migracije — nije potrebno ručno postavljanje baze.

- API: http://localhost:5080/swagger
- Auth servis: http://localhost:5081/swagger

Registrirajte se / prijavite putem auth servisa, a zatim dobiveni JWT koristite kao `Bearer` token prema API-ju.

Zaustavljanje:
```bash
docker compose down
```

Konfiguracija (DB podaci, JWT ključ) nalazi se u `.env` datoteci u korijenu repozitorija, a dijele je oba servisa preko `compose.yaml`.

## Pokretanje bez Dockera

Potrebna je lokalna Postgres instanca koja odgovara connection stringu u `appsettings.json` svakog projekta, zatim:

```bash
dotnet run --project RydePlannr.AuthService   # http://localhost:5121
dotnet run --project RydePlannr.API           # http://localhost:5120
```

## Testovi

```bash
dotnet test RydePlannr.Tests
```
