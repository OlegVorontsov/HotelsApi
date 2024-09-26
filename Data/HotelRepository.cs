
using HotelsApi.Data;
using Microsoft.EntityFrameworkCore;

public class HotelRepository : IHotelRepository
{
  private readonly HotelsDbContext _db;
  public HotelRepository(HotelsDbContext db) => _db = db;
  public async Task<List<Hotel>> GetHotelListAsync() => await _db.Hotels.ToListAsync();
  public async Task<List<Hotel>> GetHotelsByNameAsync(string hotelName) =>
                await _db.Hotels.Where(h => h.Name.Contains(hotelName)).ToListAsync();
  public async Task<Hotel> GetHotelByIdAsync(int id) => await _db.Hotels.FindAsync(new object[] { id });
  public async Task InsertHotelAsync(Hotel hotel) => await _db.Hotels.AddAsync(hotel);
  public async Task UpdateHotelAsync(Hotel hotel)
  {
    var hotelFromDb = await _db.Hotels.FindAsync(new object[] { hotel.Id });
    if (hotelFromDb is null) return;
    hotelFromDb.Name = hotel.Name;
    hotelFromDb.Address = hotel.Address;
    hotelFromDb.Description = hotel.Description;
    hotelFromDb.StarsCount = hotel.StarsCount;
    await _db.SaveChangesAsync();
  }
  public async Task DeleteHotelById(int id)
  {
    var hotelFromDb = await _db.Hotels.FindAsync(new object[] { id});
    if (hotelFromDb is null) return;
    _db.Hotels.Remove(hotelFromDb);
    await _db.SaveChangesAsync();
  }
  public Task SaveChangesAsync() => _db.SaveChangesAsync();

  private bool _disposed = false;
  protected virtual void Dispose(bool disposing)
  {
    if(!_disposed)
    {
      if(disposing)
      {
        _db.Dispose();
      }
    }
    _disposed = true;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}