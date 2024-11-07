using AutoMapper;
using Common.Constants;
using Common.DbEntities;
using Common.Dtos;
using Common.Interfaces;
using Common.Services;

namespace Receiver
{
    public class TaskReceiverService(IServiceBusHandler serviceBusHandler,
        DataContext dataContext,
        IMapper mapper) : IDisposable
    {

        public void RegisterReceivers()
        {
            serviceBusHandler.RegisterMessageHandler<CreateTaskDto>(
                ServiceBusConstants.CreateTaskQueue,
                CreateAsync);

            Console.WriteLine($"Queue {ServiceBusConstants.CreateTaskQueue} waits messages");

            serviceBusHandler.RegisterMessageHandler<UpdateTaskDto>(
                ServiceBusConstants.UpdateTaskQueue,
                UpdateAsync);

            Console.WriteLine($"Queue {ServiceBusConstants.UpdateTaskQueue} waits messages");
        }

        public void Dispose() => serviceBusHandler.CloseAllClients();

        private async Task CreateAsync(CreateTaskDto? dto)
        {
            if (dto == null)
                throw new NullReferenceException($"{nameof(CreateAsync)}: dto is null");

            var dbEntity = mapper.Map<TaskEntity>(dto);

            await dataContext.Tasks.AddAsync(dbEntity);
            await dataContext.SaveChangesAsync();

            Console.WriteLine($"Task created: {dto!.Name} ({dto!.Description})");
        }

        private async Task UpdateAsync(UpdateTaskDto? dto)
        {
            if (dto == null)
                throw new NullReferenceException($"{nameof(UpdateAsync)}: dto is null");

            var existingTask = await dataContext.Tasks.FindAsync(dto.Id);
            if (existingTask == null)
            {
                Console.WriteLine($"Entity (Id:{dto.Id}) not found in db");
                return;
            }

            existingTask.Status = dto.NewStatus;
            await dataContext.SaveChangesAsync();

            Console.WriteLine($"Task update: {dto.Id}; {dto.NewStatus}");
        }
    }
}
