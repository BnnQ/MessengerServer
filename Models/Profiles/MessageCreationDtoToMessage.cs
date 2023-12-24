using AutoMapper;
using MessengerServer.Entities;
using MessengerServer.Models.Dto.Message;

namespace MessengerServer.Models.Profiles;

public class MessageCreationDtoToMessage : Profile
{
    public MessageCreationDtoToMessage()
    {
        CreateMap<MessageCreationDto, Message>();
    }
}