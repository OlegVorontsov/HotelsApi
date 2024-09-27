using System.ComponentModel.DataAnnotations;

public record UserModel
{
  [Required]
  public string UserName { get; set; } = String.Empty;
  [Required]
  public string Password { get; set;} = String.Empty;
}
public record UserDto(string userName, string password);