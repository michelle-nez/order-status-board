using Microsoft.EntityFrameworkCore;
using OrderStatus.Data;
using OrderStatus.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// AddDbContextFactory, not AddDbContext - Blazor Server components are long-lived
// and can run several operations at once. Each operation gets its own context.
builder.Services.AddDbContextFactory<BoardDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BoardDatabase")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
