# AutoRent API — implemented endpoints

Base URL: `/api/v1`. JSON uses camelCase. Timestamps are ISO 8601 UTC. The OpenAPI document is served at `/openapi/v1.json` in Development.

This file lists only the routes that exist in the code. The full planned route list is in [DEVELOPMENT_SPEC.md §6](DEVELOPMENT_SPEC.md#6-api-conventions-and-proposed-endpoints).

## Authentication

Send the access token as `Authorization: Bearer <token>`. Tokens are JWTs that last 30 minutes and carry only the user's identity. Admin rights and account suspension are **checked against the database on every request**, so demoting or suspending a user takes effect immediately, even for tokens that have not expired yet.

## Endpoints

| Method | Route | Access | Success | Description |
|---|---|---|---|---|
| `POST` | `/auth/register` | Public, rate-limited | `201` `AuthResponse` | Create an account and log in. |
| `POST` | `/auth/login` | Public, rate-limited | `200` `AuthResponse` | Log in with email and password. |
| `POST` | `/auth/logout` | Active user | `204` | Ends the session on the client. Tokens are stateless, so the client must discard the token; it stays valid until it expires. |
| `GET` | `/auth/me` | Active user | `200` `UserProfile` | The current user's profile. |
| `GET` | `/admin/users?page=1&pageSize=20&search=` | Admin | `200` `PagedResult<AdminUser>` | List users, optionally filtered by name or email. `pageSize` is at most 100. |
| `PATCH` | `/admin/users/{id}` | Admin | `200` `AdminUser` | Body `{ "isAdmin"?: bool, "isAccountActive"?: bool }`. An admin cannot demote or suspend themselves. |

"Active user" means a valid token whose account is not suspended (`IsAccountActive = true`).

### Shapes

```jsonc
// AuthResponse
{ "accessToken": "eyJ…", "expiresAt": "2026-11-20T10:30:00+00:00", "user": UserProfile }

// UserProfile
{ "id": 1, "email": "admin@autorent.local", "fullName": "AutoRent Admin", "phone": "+40 …",
  "isAdmin": true, "addressLine": "…", "city": "Craiova", "county": "Dolj", "postalCode": "200585", "country": "Romania" }

// AdminUser
{ "id": 2, "email": "ana.popescu@autorent.local", "fullName": "Ana Popescu", "phone": "+40 …",
  "city": "Craiova", "isAdmin": false, "isAccountActive": true }

// PagedResult<T>
{ "items": [T], "page": 1, "pageSize": 20, "totalCount": 3, "totalPages": 1 }
```

Register request: `email` (required, valid, ≤ 256), `password` (8–72 characters), `fullName` (2–150), `phone` (optional).

## Errors

Every failure is an RFC 9457 `application/problem+json` body with a stable `code` and a `traceId`:

```json
{ "status": 409, "title": "An account with this email already exists.", "code": "auth.email_taken", "traceId": "0HN…" }
```

Validation failures add `errors`, keyed by camelCase field name:

```json
{ "status": 400, "code": "validation.failed", "errors": { "email": ["The Email field is not a valid e-mail address."] } }
```

| Status | Code | When |
|---|---|---|
| 400 | `validation.failed` | Request body fails validation. |
| 400 | `pagination.invalid` / `admin.empty_update` | Invalid paging values, or an empty PATCH body. |
| 401 | `auth.unauthenticated` | Missing, malformed, expired or wrongly signed token. |
| 401 | `auth.invalid_credentials` | Wrong email or password (the same message for both). |
| 403 | `auth.account_suspended` | Correct credentials, but the account is suspended. |
| 403 | `auth.forbidden` | Authenticated, but not allowed (suspended, or not an admin). |
| 404 | `user.not_found` | The user does not exist. |
| 409 | `auth.email_taken` | The email is already registered (case-insensitive). |
| 422 | `admin.self_lockout` | An admin tried to remove their own admin rights or suspend themselves. |
| 429 | `auth.rate_limited` | Too many login/register attempts from one IP (default 10 per minute). |
| 500 | `server.error` | Unexpected error. No internal details are returned. |
