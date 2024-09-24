using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var hotels = new List<Hotel>();

app.MapGet("/hotels", () => hotels);
app.MapGet("/hotels/{id}", (int id) => hotels.FirstOrDefault(h => h.Id == id));
app.MapPost("/hotels", (Hotel hotel) => hotels.Add(hotel));
app.MapPut("/hotels", (Hotel hotel) =>
  {
    var index = hotels.FindIndex(h => h.Id == hotel.Id);
    if (index < 0) throw new Exception("Not found");
    hotels[index] = hotel;
  });
app.MapDelete("/hotels/{id}", (int id) =>
{
    var index = hotels.FindIndex(h => h.Id == id);
    if (index < 0) throw new Exception("Not found");
    hotels.RemoveAt(index);
});

app.Run();

public class Hotel
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; }
  public string Address { get; set; }
  public int StarsCount { get; set; }
}