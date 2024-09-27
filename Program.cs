using System.Text;
using HotelsApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelsDbContext>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(HotelsDbContext)));
});

builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new()
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"],
      ValidAudience = builder.Configuration["Jwt:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
  });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelsDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/login", [AllowAnonymous] async (HttpContext context, ITokenService tokenService, IUserRepository userRepository) => 
{
  UserModel userModel = new ()
  {
    UserName = context.Request.Query["username"],
    Password = context.Request.Query["password"]
  };
    var userDto = userRepository.GetUser(userModel);
    if (userDto == null) return Results.Unauthorized();
    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
            builder.Configuration["Jwt:Issuer"], userDto);
    return Results.Ok(token);
});

app.MapGet("/hotels", [Authorize] async (IHotelRepository repository) =>
    Results.Ok(await repository.GetHotelListAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", [Authorize] async (int id, IHotelRepository repository) => 
    await repository.GetHotelByIdAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotelById")
    .WithTags("Getters");

app.MapGet("/hotels/search/name/{query}", [Authorize] async (string query, IHotelRepository repository) =>
    await repository.GetHotelsByNameAsync(query) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters");

app.MapPost("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
  {
    await repository.InsertHotelAsync(hotel);
    await repository.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
      await repository.UpdateHotelAsync(hotel);
      await repository.SaveChangesAsync();
      return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

app.MapDelete("/hotels/{id}", [Authorize] async (int id, IHotelRepository repository) =>
    {
      await repository.DeleteHotelById(id);
      await repository.SaveChangesAsync();
      return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.UseHttpsRedirection();

app.Run();
