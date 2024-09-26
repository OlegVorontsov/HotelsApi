using System.IO.Compression;
using HotelsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelsDbContext>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(HotelsDbContext)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelsDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (HotelsDbContext db) => await db.Hotels.ToListAsync());

app.MapGet("/hotels/{id}", async (int id, HotelsDbContext db) => 
  await db.Hotels.FirstOrDefaultAsync(h => h.Id == id) is Hotel hotel
  ? Results.Ok(hotel)
  : Results.NotFound());

app.MapPost("/hotels", async ([FromBody] Hotel hotel, HotelsDbContext db) =>
  {
    db.Hotels.Add(hotel);
    await db.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
  });

app.MapPut("/hotels", async ([FromBody] Hotel hotel, HotelsDbContext db) =>
  {
    var hotelFromDb = await db.Hotels.FindAsync(new object[] { hotel.Id });
    if (hotelFromDb is null) return Results.NotFound();
    hotelFromDb.Name = hotel.Name;
    hotelFromDb.Address = hotel.Address;
    hotelFromDb.Description = hotel.Description;
    hotelFromDb.StarsCount = hotel.StarsCount;
    await db.SaveChangesAsync();
    return Results.NoContent();
  });
app.MapDelete("/hotels/{id}", async (int id, HotelsDbContext db) =>
{
  var hotelFromDb = await db.Hotels.FindAsync(new object[] { id});
  if (hotelFromDb is null) return Results.NotFound();
  db.Hotels.Remove(hotelFromDb);
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();

public class Hotel
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; }
  public string Address { get; set; }
  public int StarsCount { get; set; }
}