# Order Status Board

See where every order stands. Place an order, move it along its lifecycle, and
read the whole pipeline at a glance. Built to practice EF Core relationships and
lookup-table design on SQL Server.

![Order board](screenshots/order-board.png)

## Stack

- .NET 10, Blazor Server (interactive server rendering)
- Entity Framework Core 10
- SQL Server (LocalDB in development)

## What it does

- Orders, customers, and statuses across three related SQL Server tables
- Status is a **lookup table**, so a stage is never typed two different ways
- Board columns are driven by the lookup's `SortOrder`, not hard-coded in markup
- Move an order forward or back a column; the end columns disable the arrow that
  has nowhere to go
- Unique index on the order number, enforced by the database rather than the screen
- Cancelling an order hides it and keeps the record
- Responsive - the columns stack instead of scrolling sideways on a phone

## The schema

```
dbo.Customers
    Id          int, primary key, identity
    Name        nvarchar(120), not null
    Email       nvarchar(200), not null

dbo.OrderStates                      -- the lookup
    Id          int, primary key, identity
    Name        nvarchar(40), not null
    SortOrder   int, not null        -- drives the board column order
    Accent      nvarchar(20), not null

dbo.Orders
    Id            int, primary key, identity
    OrderNumber   nvarchar(40), not null, UNIQUE index
    Total         decimal(18,2), not null
    Channel       nvarchar(40), not null
    PlacedUtc     datetime2, not null
    IsCancelled   bit, not null
    CustomerId    int, not null, foreign key -> Customers.Id
    OrderStateId  int, not null, foreign key -> OrderStates.Id
```

`Total` is `decimal(18,2)`, not a floating point type - money has to be exact.
Both foreign keys use `DeleteBehavior.Restrict`, so a customer or a status that
still has orders pointing at it cannot be deleted out from under them. The status
list and a few customers are seeded by the migration, so the board has its columns
on a fresh database.

## Why a lookup table

Storing the stage as free text means "Shipped", "shipped", and "Shiped" all end up
in the same column of the same table, and no query can group them. A lookup table
makes the set of valid stages a database constraint, gives each one a stable
`SortOrder` for display, and means renaming a stage is one row update rather than a
bulk string replace.

## Project layout

| Project | What it holds |
|---|---|
| `OrderStatus.Web` | Blazor Server app - the screens |
| `OrderStatus.Data` | Models, `BoardDbContext`, and EF Core migrations |

The reference points one way only: Web references Data, never the reverse. The
context is registered with `AddDbContextFactory`, not `AddDbContext`, because
Blazor Server components are long-lived and several can run at once.

## Running it locally

1. Open `OrderStatusBoard.sln` in Visual Studio 2026.
2. Right-click `OrderStatus.Web` and choose **Manage User Secrets**.
3. Add a `ConnectionStrings:BoardDatabase` value pointing at your local SQL Server
   LocalDB instance, with the database named `OrderStatusBoard` and a trusted
   connection. See `appsettings.json` for the setting's shape.
4. In the Package Manager Console, set **Default project** to `OrderStatus.Data`
   and run `Update-Database`.
5. Run the project and open `/orders`.

The connection string lives in User Secrets only. `appsettings.json` keeps a blank
placeholder so the setting's shape is documented without a value in the repo.

## What I would do next

- Filter the board by channel or customer
- A view for cancelled orders, with a way to reinstate one
- Order lines, so an order is master-detail rather than a single total
- Unit tests over the move logic

---

Self-directed portfolio project.
