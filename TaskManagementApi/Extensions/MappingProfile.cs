using AutoMapper;
using Common.DbEntities;
using Common.Dtos;
using TaskManagementApi.Models;

namespace TaskManagementApi.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateTaskArgs, CreateTaskDto>();
            CreateMap<UpdateTaskArgs, UpdateTaskDto>();
            CreateMap<TaskEntity, TaskView>();
        }
    }
}
