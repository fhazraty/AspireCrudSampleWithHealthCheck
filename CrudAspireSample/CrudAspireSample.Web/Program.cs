using CrudAspireSample.Web;
using CrudAspireSample.Web.Components;
using Microsoft.EntityFrameworkCore;
using Web.Contracts;
using Web.Data;
using Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://apiservice"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crud API v1");
    c.RoutePrefix = "swagger";
});


app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapDefaultEndpoints();


// Create
app.MapPost("/people", async (AppDbContext db, PersonCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        return Results.BadRequest("FirstName and LastName are required.");

    var person = new Person { FirstName = dto.FirstName.Trim(), LastName = dto.LastName.Trim() };
    db.People.Add(person);
    await db.SaveChangesAsync();

    var read = new PersonReadDto(person.Id, person.FirstName, person.LastName);
    return Results.Created($"/people/{person.Id}", read);
});

// Read All (با paging ساده)
app.MapGet("/people", async (AppDbContext db, int page = 1, int pageSize = 20) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

    var query = db.People.AsNoTracking();
    var total = await query.CountAsync();
    var items = await query
        .OrderBy(p => p.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new PersonReadDto(p.Id, p.FirstName, p.LastName))
        .ToListAsync();

    return Results.Ok(new { total, page, pageSize, items });
});

// Read By Id
app.MapGet("/people/{id:int}", async (AppDbContext db, int id) =>
{
    var p = await db.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    return p is null
        ? Results.NotFound()
        : Results.Ok(new PersonReadDto(p.Id, p.FirstName, p.LastName));
});

// Update
app.MapPut("/people/{id:int}", async (AppDbContext db, int id, PersonUpdateDto dto) =>
{
    var person = await db.People.FindAsync(id);
    if (person is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        return Results.BadRequest("FirstName and LastName are required.");

    person.FirstName = dto.FirstName.Trim();
    person.LastName = dto.LastName.Trim();

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete
app.MapDelete("/people/{id:int}", async (AppDbContext db, int id) =>
{
    var person = await db.People.FindAsync(id);
    if (person is null) return Results.NotFound();

    db.People.Remove(person);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
