# Favorite Quotes Web API

This is an ASP.NET 8 C# Web API for managing users, books, and quotes. It is designed to work as the backend for an [Angular frontend]("https://github.com/lundbergsimon/favorite-quotes-frontend") and supports secure authentication using JWT and refresh tokens (stored in HTTP-only cookies).

## Features
- User registration and login with JWT authentication
- Secure refresh token handling via HTTP-only cookies
- CRUD operations for books and quotes
- CORS configured for Angular frontend (localhost and production)
- Dockerfile for containerized production deployment

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- (Optional) [Docker](https://www.docker.com/)

### Local Development
1. Clone the repository:
   ```sh
   git clone https://github.com/lundbergsimon/favorite-quotes-webapi
   cd favorite-quotes-webapi/src
   ```
2. Run the API:
   ```sh
   dotnet run
   ```
3. The API will be available at `http://localhost:5173` (see `launchSettings.json`).
4. The [Angular frontend]("https://github.com/lundbergsimon/favorite-quotes-frontend") should be configured to use this API URL.

### API Endpoints
- `POST /auth/register` — Register a new user and login
- `POST /auth/login` — Login and receive JWT + refresh token cookie
- `GET /books` — List all books
- `POST /books` — Add a new book
- `PUT /books/{id}` — Update a book
- `DELETE /books/{id}` — Delete a book
- `GET /quotes` — List all quotes
- `POST /quotes` — Add a new quote
- `DELETE /quotes/{id}` — Delete a quote

### Docker
To build and run the API in a container:
```sh
docker build -t favorite-quotes-webapi .
docker run -d -p 5173:5173 favorite-quotes-webapi
```

## Configuration
- `appsettings.json` — Main configuration
