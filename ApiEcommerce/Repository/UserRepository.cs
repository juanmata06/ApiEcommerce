using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos.ApplicationUser;
using ApiEcommerce.Models.Dtos.User;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly string? secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(
        ApplicationDbContext db,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper
    )
    {
        _db = db;
        _configuration = configuration;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public ApplicationUser? GetUserById(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
    }

    public bool UserExistsByUserName(string userName)
    {
        return _db.Users.Any(u => u.Username.ToLower().Trim() == userName.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Username))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username is required"
            };
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(
            u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim()
        );
        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username not found"
            };
        }

        if (userLoginDto.Password == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Password required"
            };
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (!isValid)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Credentials are not correct"
            };
        }

        var handlerToken = new JwtSecurityTokenHandler();
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("SecretKey has not been configured");
        }
        var roles = await _userManager.GetRolesAsync(user);
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),
            }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = _configuration["ApiSettings:Issuer"],
            Audience = _configuration["ApiSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);

        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = _mapper.Map<UserDataDto>(user),
            Message = "User logged succesfully!"
        };
    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {
        if (string.IsNullOrEmpty(createUserDto.Username))
        {
            throw new InvalidOperationException("Username is required");
        }

        if (createUserDto.Password == null)
        {
            throw new InvalidOperationException("Password is required");
        }

        var user = new ApplicationUser()
        {
            UserName = createUserDto.Username,
            Email = createUserDto.Username,
            NormalizedEmail = createUserDto.Username.ToUpper(),
            Name = createUserDto.Name,
        };
        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (result.Succeeded)
        {
            var userRole = createUserDto.Role ?? "User";
            var roleExists = await _roleManager.RoleExistsAsync(userRole);
            if (!roleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _roleManager.CreateAsync(identityRole);
            }
            await _userManager.AddToRoleAsync(user, userRole);
            var createdUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == createUserDto.Username);
            return _mapper.Map<UserDataDto>(createdUser);
        }

        throw new InvalidOperationException("An error has occurred during user registration");
    }

    public async Task<bool> SaveAsync()
    {
        // Proper async implementation for this repository only
        return await _db.SaveChangesAsync() >= 0;
    }
}
