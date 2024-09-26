public interface IHotelRepository : IDisposable
{
  Task<List<Hotel>> GetHotelListAsync();
  Task<List<Hotel>> GetHotelsByNameAsync(string hotelName);
  Task<Hotel> GetHotelByIdAsync(int id);
  Task InsertHotelAsync(Hotel hotel);
  Task UpdateHotelAsync(Hotel hotel);
  Task DeleteHotelById(int id);
  Task SaveChangesAsync();
}