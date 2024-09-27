using System.Text;
using HotelsApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder.Services);

var app = builder.Build();

Configure(app);

var Apis = app.Services.GetServices<IAPI>();
foreach(var api in Apis)
{
  if (api is null) throw new InvalidOperationException("Api not found");
  api.Register(app);
}

app.Run();

void RegisterServices(IServiceCollection services)
{
  services.AddEndpointsApiExplorer();
  services.AddSwaggerGen();

  services.AddDbContext<HotelsDbContext>(options =>
  {
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(HotelsDbContext)));
  });

  services.AddScoped<IHotelRepository, HotelRepository>();
  services.AddSingleton<ITokenService>(new TokenService());
  services.AddSingleton<IUserRepository>(new UserRepository());
  services.AddAuthorization();
  services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
  services.AddTransient<IAPI, HotelsAPI>();
  services.AddTransient<IAPI, AuthAPI>();
}

void Configure (WebApplication app)
{
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
  app.UseHttpsRedirection();
}
