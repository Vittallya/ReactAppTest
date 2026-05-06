using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("create-order", opt =>
    {
        opt.PermitLimit = 2;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    
    options.AddFixedWindowLimiter(policyName: "read-all-data", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.AddTokenBucketLimiter(policyName: "read-data", opt =>
    {
        opt.TokenLimit = 20;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// builder.Services.Configure<ForwardedHeadersOptions>(options =>
// {
//     options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
//     options.KnownIPNetworks.Clear(); 
//     options.KnownProxies.Clear();
// });

var app = builder.Build();

//app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseDefaultFiles();
//app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRateLimiter();


app.MapPost("/api/orders/create", async ([FromBody] Order order, [FromServices] IValidator<Order> validator, [FromServices] IOrderRepository repository) =>
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
}).RequireRateLimiting("create-order");


app.MapGet("/api/orders", async ([FromQuery] int? lastId, [FromQuery] int? limit, [FromServices] IOrderRepository repository) =>
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
}).RequireRateLimiting("read-all-data");

app.MapGet("/api/orders/{id}", async (int id, [FromServices] IOrderRepository repository) =>
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
}).RequireRateLimiting("read-data");


app.MapFallbackToFile("/index.html");

app.Services.GetRequiredService<DbService>().InitDb();
app.Run();