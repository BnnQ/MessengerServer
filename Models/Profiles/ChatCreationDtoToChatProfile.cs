using AutoMapper;
using MessengerServer.Entities;
using MessengerServer.Models.Dto.Chat;

namespace MessengerServer.Models.Profiles;

public class ChatCreationDtoToChatProfile : Profile
{
    public ChatCreationDtoToChatProfile()
    {
        CreateMap<ChatCreationDto, Chat>();
    }
}