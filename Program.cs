using HotelsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelsDbContext>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(HotelsDbContext)));
});

builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelsDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) => Results.Ok(await repository.GetHotelListAsync()));

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) => 
  await repository.GetHotelByIdAsync(id) is Hotel hotel
  ? Results.Ok(hotel)
  : Results.NotFound());

app.MapPost("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
  {
    await repository.InsertHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
  });

app.MapPut("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
  {
    await repository.UpdateHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.NoContent();
  });
app.MapDelete("/hotels/{id}", async (int id, IHotelRepository repository) =>
{
  await repository.DeleteHotelById(id);
  await repository.SaveChangesAsync();
  return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();
