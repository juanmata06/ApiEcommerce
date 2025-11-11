using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos.ApplicationUser;
using ApiEcommerce.Models.Dtos.User;

namespace ApiEcommerce.Repository.IRepository;

public interface IUserRepository
{
    ICollection<ApplicationUser> GetUsers();
    ApplicationUser? GetUserById(string id);
    bool UserExistsByUserName(string userName);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<UserDataDto> Register(CreateUserDto createUserDto);
    Task<bool> SaveAsync();
}
