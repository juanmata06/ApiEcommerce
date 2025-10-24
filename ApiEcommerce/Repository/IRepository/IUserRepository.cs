using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos.User;

namespace ApiEcommerce.Repository.IRepository;

public interface IUserRepository
{
    ICollection<User> GetUsers();
    User? GetUserById(int id);
    bool UserExistsByUserName(string userName);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<User> Register(CreateUserDto createUserDto);
    bool Save();
}
