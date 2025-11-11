using System;

using ApiEcommerce.Models.Dtos.ApplicationUser;

namespace ApiEcommerce.Models.Dtos.User;

public class UserLoginResponseDto
{
    public UserDataDto? User { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}
