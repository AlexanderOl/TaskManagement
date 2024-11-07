using AutoMapper;
using Common.DbEntities;
using Common.Dtos;

namespace Receiver
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateTaskDto, TaskEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
