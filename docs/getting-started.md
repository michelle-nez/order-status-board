# Getting started

From a fresh clone to a running board. Roughly ten minutes, most of it waiting for
NuGet.

## Prerequisites

| Requirement | Notes |
|---|---|
| Visual Studio 2026 | With the **ASP.NET and web development** workload |
| .NET 10 SDK | Installed with that workload; `dotnet --version` should report 10.x |
| SQL Server LocalDB | Ships with the workload. Any SQL Server instance works — only the connection string changes |

Check LocalDB is present:

```powershell
sqllocaldb info
```

`MSSQLLocalDB` should be listed. If the command is not found, add **SQL Server Express
LocalDB** through the Visual Studio Installer under Individual components.

## 1. Clone and open

```bash
git clone https://github.com/michelle-nez/order-status-board.git
cd order-status-board
```

Open `OrderStatusBoard.sln`. Visual Studio restores the NuGet packages on load;
`OrderStatus.Web` is already the startup project.

## 2. Add the connection string

**The app will not run until this is done.** `appsettings.json` ships the key with an
empty value so the shape is documented without a credential in the repository:

```json
"ConnectionStrings": {
  "BoardDatabase": ""
}
```

The real value goes in **User Secrets**, which live outside the repository in your
Windows user profile and are never committed.

**In Visual Studio** — right-click `OrderStatus.Web` → **Manage User Secrets**, then:

```json
{
  "ConnectionStrings": {
    "BoardDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=OrderStatusBoard;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Or from the CLI**, run from the solution folder:

```bash
dotnet user-secrets set "ConnectionStrings:BoardDatabase" "Server=(localdb)\MSSQLLocalDB;Database=OrderStatusBoard;Trusted_Connection=True;TrustServerCertificate=True" --project OrderStatus.Web
```

`Trusted_Connection=True` means Windows authentication, so there is no password to
store anywhere — the development setup has no credential to leak.

## 3. Create the database

Nothing creates it automatically. There is no `Migrate()` or `EnsureCreated()` call in
`Program.cs`, so a fresh clone has no schema until the migration is applied by hand.

**Package Manager Console** — set **Default project** to `OrderStatus.Data`, and leave
the startup project as `OrderStatus.Web`:

```powershell
Update-Database
```

**Or from the CLI**, naming both projects:

```bash
dotnet ef database update --project OrderStatus.Data --startup-project OrderStatus.Web
```

Both have to be named because the migrations live in `OrderStatus.Data` while the EF
Core tools and the connection string live in `OrderStatus.Web`.

If `dotnet ef` is not recognized:

```bash
dotnet tool install --global dotnet-ef
```

This creates three tables, four indexes, both foreign keys, and inserts five statuses
and three customers. Full detail in [database.md](database.md).

## 4. Run it

Press **F5**, or:

```bash
dotnet run --project OrderStatus.Web
```

| Profile | URL |
|---|---|
| `http` | http://localhost:5090 |
| `https` | https://localhost:7203 (also serves 5090) |

Both profiles set `ASPNETCORE_ENVIRONMENT=Development`.

On first run over HTTPS the development certificate may need trusting:

```bash
dotnet dev-certs https --trust
```

## 5. Check it actually works

A fresh database has **five statuses, three customers and no orders**, so:

1. Open `/orders`. You should see the **empty state**, "No open orders" — but note the
   board itself has structure because the statuses are seeded. If you see no columns at
   all, the migration did not run.
2. Click **New order**. The customer dropdown should list RiteAV, Ultra Spec Cables and
   Wallplate City, and Status should default to **New**.
3. Save an order. It appears in the **New** column, and the count and pipeline total
   above the board update.
4. Press the **right arrow** on the card. It moves to Picking. On a card in the first
   column the left arrow is disabled; in the last column the right arrow is disabled.
5. Add a second order **reusing the same order number**. It should be rejected with
   "Order number 'X' is already in use." — that message means the unique index is
   doing its job.
6. Open an order and **cancel** it. It disappears from the board, but the row is still
   there — soft delete, not a delete.

If all six behave, the app, the lookup table and the database are wired up correctly.

## Current deployment state

**This application is not deployed anywhere, and has no deployment configuration.**

Verified in the repository: no publish profile (`.pubxml`), no `Dockerfile`, no
`.github/workflows`, no `appsettings.Production.json`. It runs on a developer machine
against LocalDB.

## Optional future deployment

**Nothing here is implemented.** It is recorded so the gap is explicit.

Because this is Blazor Server, three constraints apply:

- It needs a real .NET host holding a **live SignalR connection**. Static hosts
  (GitHub Pages, Netlify) cannot run it at all.
- It needs a **reachable SQL Server**, not LocalDB, which is developer-only.
- **Sticky sessions** matter if it is ever scaled past one instance, because each
  circuit is bound to the server that created it.

A minimal deployment would need a .NET 10 host, a SQL Server database there with the
connection string supplied through the host's configuration or environment variables
(**never the repository**), the migration applied against it — by running
`dotnet ef database update` or generating a script with `dotnet ef migrations script`
— and `ASPNETCORE_ENVIRONMENT` set to `Production`, which is what switches on
`UseExceptionHandler` and HSTS.

Applying migrations automatically at startup is deliberately not done; it would let a
deploy alter a production schema without anyone choosing to.

## Where to go next

| Document | Covers |
|---|---|
| [architecture.md](architecture.md) | Projects, layers, rendering model, how the lookup drives the board |
| [database.md](database.md) | Schema, entities, relationships, migrations, seed data, ER diagram |
