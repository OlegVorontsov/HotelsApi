using HotelsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelsDbContext>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(HotelsDbContext)));
});

builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelsDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) =>
    Results.Ok(await repository.GetHotelListAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) => 
    await repository.GetHotelByIdAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotelById")
    .WithTags("Getters");

app.MapGet("/hotels/search/name/{query}", async (string query, IHotelRepository repository) =>
    await repository.GetHotelsByNameAsync(query) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters");

app.MapPost("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
  {
    await repository.InsertHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
      await repository.UpdateHotelAsync(hotel);
      await repository.SaveChangesAsync();
      return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

app.MapDelete("/hotels/{id}", async (int id, IHotelRepository repository) =>
    {
      await repository.DeleteHotelById(id);
      await repository.SaveChangesAsync();
      return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");;

app.UseHttpsRedirection();

app.Run();
