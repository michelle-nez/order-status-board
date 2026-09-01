# Configuration

Every setting this application reads, where it comes from, and which ones matter.

The configuration surface is deliberately small: **one setting has to be supplied**
(the connection string) and everything else has a working default.

## Where settings come from

ASP.NET Core layers configuration sources, and later sources win:

1. `appsettings.json`
2. `appsettings.{Environment}.json` — here, `appsettings.Development.json`
3. **User Secrets — Development environment only**
4. Environment variables
5. Command-line arguments

### The precedence trap

**User Secrets override `appsettings.json`, and they are only loaded in Development.**

Two consequences that cause real confusion:

- Putting a connection string into `appsettings.json` while a User Secrets value exists
  appears to do nothing. The secret is winning. Change the secret, not the file.
- In **Production**, User Secrets are not loaded at all. A deployed instance must get
  its connection string from an environment variable or the host's own configuration.
  There is no production config file in this repository.

## appsettings.json

The committed file, in full:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "BoardDatabase": ""
  }
}
```

| Setting | Meaning |
|---|---|
| `Logging:LogLevel:Default` | `Information` — application log level. This is what surfaces the save-failure log written by `OrderEdit` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` — quiets per-request framework noise |
| `AllowedHosts` | `*`, the framework default. Worth narrowing to real hostnames if ever deployed |
| `ConnectionStrings:BoardDatabase` | **Intentionally empty.** The key documents the shape; the value lives in User Secrets |

**The empty connection string is the whole security posture of this repo.** The key
exists so a developer knows what to supply, and no credential is ever committed. Do
not fill it in — a value here would be committed the moment it is saved.

## appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

This overrides the base file with **identical values**, so it currently changes
nothing. It is the stock template file, left in place. It becomes useful the moment
development needs different logging — for example
`"Microsoft.EntityFrameworkCore.Database.Command": "Information"` to watch the SQL
EF Core generates, which is genuinely interesting in this app because the board runs
two queries per load.

## The connection string

| | |
|---|---|
| Key | `ConnectionStrings:BoardDatabase` |
| Read by | `Program.cs`, via `builder.Configuration.GetConnectionString("BoardDatabase")` |
| Development source | User Secrets |
| Development value | `Server=(localdb)\MSSQLLocalDB;Database=OrderStatusBoard;Trusted_Connection=True;TrustServerCertificate=True` |

`Trusted_Connection=True` uses Windows authentication, so the development setup has
**no password anywhere** — nothing to leak, nothing to rotate.

The provider is SQL Server on both sides. Moving to a real server changes the
connection string and nothing else — not the provider, not the migrations, not the
model.

Setting it is covered step by step in
[getting-started.md](getting-started.md#2-add-the-connection-string).

## User Secrets

`OrderStatus.Web.csproj` carries the id that ties the project to its secrets file:

```xml
<UserSecretsId>6c2026ff-e6e0-4810-9d2d-08a4df04a9da</UserSecretsId>
```

The id is not a secret — it is only a folder name. The file itself lives outside the
repository at:

```
%APPDATA%\Microsoft\UserSecrets\6c2026ff-e6e0-4810-9d2d-08a4df04a9da\secrets.json
```

It is per-machine and per-Windows-user, so it does not travel with a clone. Every
developer sets their own, which is the point.

## Environments

`ASPNETCORE_ENVIRONMENT` is the only environment variable this app cares about. Both
launch profiles set it to `Development`.

What actually changes, from `Program.cs`:

| | Development | Production |
|---|---|---|
| Unhandled exceptions | Developer exception page, full stack trace | `/Error` page, no detail |
| HSTS | off | on |
| User Secrets | loaded | **not loaded** |

Everything else — routing, antiforgery, HTTPS redirection, status-code re-execution —
is registered unconditionally.

There is no `appsettings.Production.json` in the repository.

## Launch profiles

From `OrderStatus.Web/Properties/launchSettings.json`:

| Profile | URLs | Environment |
|---|---|---|
| `http` | http://localhost:5090 | Development |
| `https` | https://localhost:7203 and http://localhost:5090 | Development |

`launchSettings.json` is a **local development file only** — it is not read when the
app runs outside Visual Studio or `dotnet run`.

## Configuration that lives in the database, not in a file

Worth calling out, because it is the point of the app and it is easy to go looking in
the wrong place:

**The board's columns are configured in the `OrderStates` table**, not in
`appsettings.json` and not in markup. The column list, their left-to-right order and
their accent colors are all rows. Changing them is a data change or a migration — see
[database.md](database.md#seed-data).

The one presentation setting that *is* in code is the channel list
(`Shopify`, `Amazon`, `Walmart`, `eBay`, `Phone`), a `static readonly string[]` in
`OrderEdit.razor`. It is a short fixed list that nothing else depends on, so it never
earned a table.

## Build-time settings

Not runtime configuration, but they change behavior.

`OrderStatus.Web.csproj`:

| Property | Value | Effect |
|---|---|---|
| `TargetFramework` | `net10.0` | |
| `Nullable` | `enable` | Nullable reference type warnings on |
| `ImplicitUsings` | `enable` | Common namespaces available without `using` |
| `BlazorDisableThrowNavigationException` | `true` | Stops `NavigateTo` throwing during a lifecycle method — which the unknown-id redirect relies on |

`OrderStatus.Data.csproj` sets the same first three.

## What is not configured

Stated explicitly, because their absence is easy to mistake for something undocumented:

- **No authentication or authorization** — no identity, no cookie, no JWT, no config
- **No external services** — no API keys, no email, no storage, no message queue
- **No Swagger/OpenAPI** — there is no HTTP API in this project
- **No feature flags, health checks, or CORS policy**
- **No custom logging providers** — console logging only, at the levels above

The complete list of things that must be supplied to run this app is: **the connection
string.**

## Recommendations

Not implemented — listed so the gap stays visible.

| Recommendation | Why |
|---|---|
| Narrow `AllowedHosts` if deployed | `*` accepts any Host header |
| Add `appsettings.Production.json` if deployed | No production config exists at all today |
| Add EF Core command logging to the Development file | Would make the redundant override earn its place |
