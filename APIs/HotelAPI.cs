using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HotelsAPI : IAPI
{
  public void Register(WebApplication app)
  {
    app.MapGet("/hotels", GetHotels)
        .Produces<List<Hotel>>(StatusCodes.Status200OK)
        .WithName("GetAllHotels")
        .WithTags("Getters");

    app.MapGet("/hotels/{id}", GetHotelById)
        .Produces<Hotel>(StatusCodes.Status200OK)
        .WithName("GetHotelById")
        .WithTags("Getters");

    app.MapGet("/hotels/search/name/{query}", GetHotelByName)
        .Produces<List<Hotel>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("SearchHotels")
        .WithTags("Getters");

    app.MapPost("/hotels", InsertHotel)
        .Accepts<Hotel>("application/json")
        .Produces<Hotel>(StatusCodes.Status201Created)
        .WithName("CreateHotel")
        .WithTags("Creators");

    app.MapPut("/hotels", UpdateHotel)
        .Accepts<Hotel>("application/json")
        .WithName("UpdateHotel")
        .WithTags("Updaters");

    app.MapDelete("/hotels/{id}", DeleteHotel)
        .WithName("DeleteHotel")
        .WithTags("Deleters");
  }
  [Authorize]
  private async Task<IResult> GetHotels (IHotelRepository repository) =>
          Results.Ok(await repository.GetHotelListAsync());

  [Authorize]
  private async Task<IResult> GetHotelById (int id, IHotelRepository repository) => 
        await repository.GetHotelByIdAsync(id) is Hotel hotel
        ? Results.Ok(hotel)
        : Results.NotFound();

  [Authorize]
  private async Task<IResult> GetHotelByName (string query, IHotelRepository repository) =>
        await repository.GetHotelsByNameAsync(query) is IEnumerable<Hotel> hotels
        ? Results.Ok(hotels)
        : Results.NotFound(Array.Empty<Hotel>);

  [Authorize]
  private async Task<IResult> InsertHotel ([FromBody] Hotel hotel, IHotelRepository repository)
  {
    await repository.InsertHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
  }

  [Authorize]
  async Task<IResult> UpdateHotel ([FromBody] Hotel hotel, IHotelRepository repository)
  {
    await repository.UpdateHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.NoContent();
  }
  [Authorize]
  async Task<IResult> DeleteHotel (int id, IHotelRepository repository)
  {
    await repository.DeleteHotelById(id);
    await repository.SaveChangesAsync();
    return Results.NoContent();
  }
}