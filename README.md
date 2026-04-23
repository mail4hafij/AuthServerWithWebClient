# AuthServerWithWebClient

A .NET 8 reference implementation of stateless JWT authentication with two services — an **Auth Server** that issues tokens and a **Web Client** API that validates them.

---

## How It Works

```
User → logs in at Auth Server (its own login page) → receives JWT
User → calls Web Client API with JWT → Web Client validates JWT locally
```

The Auth Server has its own login UI (Swagger in this demo) where the user authenticates and gets a JWT. The Web Client never talks to the Auth Server — it simply validates the token cryptographically using the shared signing key.

| Service | Port |
|---|---|
| AuthServer | `https://localhost:7208` |
| WebClient | `https://localhost:7024` |

---

## Running Locally

**Visual Studio** — open `AuthServerWithClientApp.sln`, set both projects as startup projects, press F5.

**.NET CLI** — two terminals:
```bash
cd AuthServer && dotnet run
cd WebClient  && dotnet run
```

---

## Try It

1. Open `http://localhost:5083/swagger` — call `POST /api/Login` with any email/password
2. Copy the `accessToken` from the response
3. Open `http://localhost:5244/swagger` — click **Authorize**, paste `Bearer <token>`
4. Call `GET /api/User` — should return `200 OK`

---

## Configuration

Both projects must share the same signing key — the WebClient uses it to verify tokens the Auth Server signed:

```json
"JWT": {
  "Key": "your-secret-key-here-and-make-sure-it-is-long",
  "Issuer": "yourauthserver.se",
  "TokenValidityInMinutes": 1440,
  "RefreshTokenValidityInDays": 7
}
```

---

## Key Concepts

**JWT** is a signed token (`header.payload.signature`). The signature is HMAC-SHA256 over the payload — any tampering invalidates it immediately, so no database lookup is needed to verify it.

**Access token** (24 h) is sent with every API request. **Refresh token** (7 days) lives in an HTTP-only cookie and is used only to obtain a new access token — keeping it out of JavaScript reduces XSS exposure.

**Stateless validation** — the Web Client verifies the signature against the shared key, checks the issuer, and checks expiry. The Auth Server is not involved at request time.

---

## Extending Toward SSO

This project implements the [OAuth2 Resource Owner Password flow](https://oauth.net/2/grant-types/password/) — the client posts credentials directly and receives a token. To evolve it into a full **Single Sign-On** system:

1. **Add a `GET /authorize` endpoint** on the Auth Server — clients redirect the browser here instead of posting credentials themselves
2. **Auth Server owns the login UI** — the user types their password only into the Auth Server, never into individual client apps
3. **Redirect back to the client with the JWT** — the client needs a dedicated callback endpoint that receives the token. If keeping it stateless, the JWT travels in the redirect. If using BFF (Backend For Frontend), return a short-lived `code` instead and exchange it server-to-server — this keeps the JWT out of the browser URL but reintroduces server-side state
4. **Add a client registry** — whitelist of allowed `client_id` + `redirect_uri` pairs to prevent redirect hijacking
5. **Keep a session cookie on the Auth Server** — so returning users are redirected silently without re-entering credentials (this is the SSO part)

Libraries like [Duende IdentityServer](https://duendesoftware.com/) or [OpenIddict](https://openiddict.com/) implement all of this on top of ASP.NET Core.
