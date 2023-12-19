using AutoMapper;
using MessengerServer.Entities;
using MessengerServer.Models.Dto.User;

namespace MessengerServer.Models.Profiles;

public class UserToUserInfoDtoProfile : Profile
{
    public UserToUserInfoDtoProfile()
    {
        CreateMap<User, UserInfoDto>();
    }
}