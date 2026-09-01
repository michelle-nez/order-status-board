# Troubleshooting

Problems you can actually hit with this application, what causes each one, and how to
confirm the fix. Grouped by when they happen.

Almost every first-run problem is one of two things: **the connection string is not
set**, or **the migration has not been applied**.

## Setup and startup

### The app builds and starts, but the board fails

Usually the connection string is missing. `appsettings.json` ships
`"BoardDatabase": ""`, and nothing supplies a value until you set a User Secret.

Confirm what the app is actually reading:

```bash
dotnet user-secrets list --project OrderStatus.Web
```

Expect a line beginning `ConnectionStrings:BoardDatabase = Server=(localdb)\...`.
Nothing listed means no secret is set — see
[getting-started.md](getting-started.md#2-add-the-connection-string).

### "Cannot open database 'OrderStatusBoard' requested by the login"

The connection string is fine; the database does not exist yet. Nothing in this app
creates it — there is no `Migrate()` or `EnsureCreated()` call in `Program.cs`.

```bash
dotnet ef database update --project OrderStatus.Data --startup-project OrderStatus.Web
```

### I edited the connection string in appsettings.json and nothing changed

**User Secrets override `appsettings.json`.** In Development the secret wins, so the
file edit is ignored. Change the secret instead, and leave the committed file empty —
a real value there would get committed.

### "Unable to create an object of type 'BoardDbContext'"

The EF Core tools are running against the wrong startup project. Migrations live in
`OrderStatus.Data`, but the tools and the connection string live in
`OrderStatus.Web`, so both must be named:

```bash
dotnet ef database update --project OrderStatus.Data --startup-project OrderStatus.Web
```

In Package Manager Console: **Default project** = `OrderStatus.Data`, **solution
startup project** = `OrderStatus.Web`. Setting only one of the two is the usual cause.

### "dotnet ef does not exist" / not recognized

```bash
dotnet tool install --global dotnet-ef
```

Then reopen the terminal so the tools path is picked up.

### `sqllocaldb info` is empty or the command is not found

LocalDB was not installed with the Visual Studio workload. Add **SQL Server Express
LocalDB** through the Visual Studio Installer → Individual components.

### The app will not start — "cannot run a class library"

`OrderStatus.Data` has been set as the startup project. It is a class library with no
entry point. Right-click **`OrderStatus.Web`** → **Set as Startup Project**.

### Port already in use

The launch profiles bind 5090 (http) and 7203 (https). Another instance is usually
still running — check for a stray `dotnet` process. Changing the port means editing
`OrderStatus.Web/Properties/launchSettings.json`.

### HTTPS certificate warnings on first run

```bash
dotnet dev-certs https --trust
```

## The board

### The board has no columns at all

**The statuses did not get seeded.** This is the failure most specific to this app:
the columns are generated from the `OrderStates` table, so with no rows there is
nothing to render — not an empty board, but a board with no structure.

```sql
SELECT Id, Name, SortOrder FROM OrderStates ORDER BY SortOrder;
```

Five rows are expected. None means the migration did not run, or ran against a
different database than the app is using. Compare the database name in your secret
against the one you migrated.

### The columns are in the wrong order

Column order comes from `OrderStates.SortOrder`, not from `Id` and not from insertion
order. Check the values are what you expect:

```sql
SELECT Name, SortOrder FROM OrderStates ORDER BY SortOrder;
```

Two rows sharing a `SortOrder` gives an unstable order between them — nothing enforces
uniqueness on that column.

### The move arrows do nothing, or an order jumps the wrong way

Moving finds the neighbouring column by `SortOrder + 1` or `- 1`, so it depends on the
sequence having **no gaps**. If the values run 1, 2, 4, 5, an order in column 2 has
nowhere to go: there is no `SortOrder` 3, so the arrow disables and the order is
stranded.

Renumber them into a contiguous run. This is also why the arrows disable at the ends —
same query, no neighbour found.

### An order vanished from the board

It was probably cancelled. The board query filters `!o.IsCancelled`, and cancelling is
a soft delete — the row is still there.

```sql
SELECT Id, OrderNumber, IsCancelled FROM Orders WHERE OrderNumber = 'X';
```

There is no screen for cancelled orders; `IsCancelled = 1` has to be flipped in SQL.

## Data and EF Core

### The customer dropdown is empty

Seed data has not been applied. The three customers are inserted by the
`InitialCreate` migration, not at runtime. Same cause as missing statuses.

### "Order number 'X' is already in use" — but that order is not on the board

**Expected behavior, not a bug.** Cancelling an order is a soft delete: the row stays
and keeps its order number, and the unique index still covers it. The board only shows
non-cancelled orders, so the conflicting one is invisible while still holding the
number.

```sql
SELECT Id, OrderNumber, IsCancelled FROM Orders WHERE OrderNumber = 'X';
```

An `IsCancelled = 1` row is the culprit. Fully explained in
[database.md](database.md#the-same-soft-delete-consequence-as-the-sku-app).

### "Could not save this order. Please try again."

The generic save failure. It means the save threw something that was **not** a
duplicate order number — a timeout, a dropped connection, or a foreign key violation.

The real exception is written to the log, so check the console output of the running
app: look for `Failed to save order <OrderNumber>` followed by the exception. The
message is deliberately vague on screen and specific in the log.

### A status or customer cannot be deleted

Working as designed. Both foreign keys use `DeleteBehavior.Restrict`, so a status
still holding orders and a customer still having orders are both protected. Move or
cancel the orders first.

### Renaming a seeded status does nothing

Seed data is applied through `HasData`, which is part of the model, not a runtime
insert. Changing a name in `OnModelCreating` requires a **new migration** to generate
the `UpdateData` statement:

```bash
dotnet ef migrations add RenameStatus --project OrderStatus.Data --startup-project OrderStatus.Web
dotnet ef database update --project OrderStatus.Data --startup-project OrderStatus.Web
```

Renaming a status directly in SQL also works and affects no orders, because orders
hold a foreign key rather than a string — which is the whole reason for the lookup
table.

### "A second operation was started on this context instance"

Should not happen here — the app uses `AddDbContextFactory` and each operation opens
its own context precisely to avoid it. If it appears, something has been changed to
`AddDbContext`, or a context is being held across `await` boundaries. The correct
pattern is:

```csharp
await using var db = await DbFactory.CreateDbContextAsync();
```

### Two people edited the same order and one change vanished

Expected with the current schema. There is no concurrency token, so the last save
wins. Noted under [architecture.md → Recommendations](architecture.md#recommendations).

## UI and runtime

### The columns scroll sideways on a phone instead of stacking

The responsive rule lives in `wwwroot/app.css`, not in MudBlazor:

```css
@media (max-width: 640px) {
    .board-scroll { grid-auto-flow: row; ... }
}
```

If columns scroll at phone width, `app.css` is not loading — check the
`<link href="@Assets["app.css"]" />` in `App.razor`. **`app.css` is live code in this
app**, unlike a stock template stylesheet; it carries the whole board grid.

### The page heading is hidden under the app bar

A `pt-*` class has been added to `MudMainContent`. That overrides the padding
MudBlazor uses to clear the fixed app bar. Put spacing on the inner `MudContainer`
instead.

### MudBlazor components render unstyled, or dialogs and snackbars never appear

One of the required pieces is missing. All must be present:

- `builder.Services.AddMudServices()` in `Program.cs`
- `@using MudBlazor` in `Components/_Imports.razor`
- `MudBlazor.min.css` and `MudBlazor.min.js` linked in `App.razor`
- `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider` and
  `MudSnackbarProvider` at the top of `MainLayout.razor`

Missing providers is the usual one: components render, but anything that overlays
silently does nothing.

### "Rejoining the server..." keeps appearing

The Blazor Server circuit dropped. Normal after the app restarts during debugging —
reload the page. If it happens repeatedly while idle, something is interrupting the
WebSocket connection: a proxy, a VPN, or an aggressive firewall.

### An unknown order id shows a blank form

It should not — `OrderEdit` redirects to `/orders` when the id matches no row. If a
blank form appears, check that `BlazorDisableThrowNavigationException` is still `true`
in `OrderStatus.Web.csproj`; the redirect happens inside a lifecycle method and
depends on it.

## Things people look for that are not here

- **Swagger / OpenAPI** — there is none. This is a Blazor Server app with no HTTP API,
  no controllers and no minimal-API endpoints. `/swagger` will 404, correctly.
- **A login page** — there is no authentication. Every page is public by design.
- **A cancelled-orders screen** — not built. Cancelled rows are only visible in SQL.
- **Order lines** — an order carries a single `Total`, not line items.

## Still stuck?

Work through the six checks at the end of
[getting-started.md](getting-started.md#5-check-it-actually-works). They isolate the
failure to setup, database, lookup table, or application in a couple of minutes.
