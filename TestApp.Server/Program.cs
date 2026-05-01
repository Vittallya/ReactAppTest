using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TestApp.Server.Infrastructure;
using TestApp.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSingleton<DbService>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connection = sp.GetRequiredService<IConfiguration>().GetConnectionString("Main");
    options.UseSqlite(connection);
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddRateLimiter(options =>
//{
//    options.AddPolicy("create_order_policy");
//});

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.MapPost("/orders/create", async ([FromBody] Order order, [FromServices] IValidator<Order> validator, [FromServices] IOrderRepository repository) =>
{
    try
    {
        var result = validator.Validate(order);
        if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary().ToList());
        await repository.CreateOrder(order, CancellationToken.None);

        return Results.Ok(order.Id);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/orders", async ([FromQuery] int? lastId, [FromQuery] int? limit, [FromServices] IOrderRepository repository) =>
{
    try
    {
        limit ??= 100;
        var (list, hasMore) = await repository.GetOrders(lastId, limit.Value, CancellationToken.None);

        return Results.Ok(new
        {
            Data = list,
            HasMore = hasMore
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/orders/{id}", async (int id, [FromServices] IOrderRepository repository) =>
{
    try
    {
        var order = await repository.FindOrder(id, CancellationToken.None);

        if (order is null)
            return Results.NotFound();

        return Results.Ok(order);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapFallbackToFile("/index.html");

app.Services.GetRequiredService<DbService>().InitDb();
app.Run();