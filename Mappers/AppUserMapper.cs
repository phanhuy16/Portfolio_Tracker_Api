using server.DTOs.Auth;
using server.Models;

namespace server.Mappers
{
    public static class AppUserMapper
    {
        public static AppUserDto MapToAppUser(this AppUser userDto)
        {
            return new AppUserDto
            {
                Id = userDto.Id,
                UserName = userDto.UserName!,
                Email = userDto.Email!,
                DateJoined = userDto.DateJoined,
                LastLogin = userDto.LastLogin,
                Currency = userDto.Currency,
                EmailNotifications = userDto.EmailNotifications
            };
        }
    }
}
