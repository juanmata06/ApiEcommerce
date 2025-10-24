using System;

namespace ApiEcommerce.Models.Dtos.User;

public class UserRegisterDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Role { get; set; }
}
