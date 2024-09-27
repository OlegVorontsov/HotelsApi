public class UserRepository : IUserRepository
{
  private List<UserDto> _users => new ()
  {
    new UserDto("Oleg","123"),
    new UserDto("Kate","123"),
    new UserDto("Pavel","123"),
  };
  public UserDto GetUser(UserModel userModel) =>
    _users.FirstOrDefault(u => 
      string.Equals(u.userName, userModel.UserName) &&
      string.Equals(u.password, userModel.Password)) ??
      throw new Exception("User not found");
}