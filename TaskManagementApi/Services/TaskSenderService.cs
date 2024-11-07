using AutoMapper;
using Common.Constants;
using Common.Dtos;
using Common.Interfaces;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;

namespace TaskManagementApi.Services
{
    public class TaskSenderService(DataContext dataContext, 
        IServiceBusHandler serviceBusHandler,
        IMapper mapper)
    {
        public async Task CreateAsync(CreateTaskArgs args)
        {
            var dto = mapper.Map<CreateTaskDto>(args);
            await serviceBusHandler.SendMessageAsync(dto, ServiceBusConstants.CreateTaskQueue);
        }

        public async Task<List<TaskView>> GetAllAsync()
        {
            var list = await dataContext.Tasks.ToListAsync();
            var views = mapper.Map<List<TaskView>>(list);

            return views;
        }

        public async Task UpdateAsync(UpdateTaskArgs args)
        {
            var dto = mapper.Map<UpdateTaskDto>(args);
            await serviceBusHandler.SendMessageAsync(dto, ServiceBusConstants.UpdateTaskQueue);
        }
    }
}
