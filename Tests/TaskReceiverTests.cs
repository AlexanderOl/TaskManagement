using AutoMapper;
using Common.DbEntities;
using Common.Dtos;
using Common.Enums;
using Common.Interfaces;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Receiver;
using System.Reflection;

namespace Tests
{
    [TestClass]
    public class TaskReceiverTests
    {
        private IMapper? _mapper;
        private Mock<IServiceBusHandler>? _serviceBusHandler;
        private DataContext? _dataContext;

        [TestInitialize]
        public void TestInitialize()
        {

            _serviceBusHandler = new Mock<IServiceBusHandler>();


            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dataContext = new DataContext(options);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();
        }

        [TestMethod]
        public void RegisterReceivers_RegisterByDefault_OK()
        {
            // ACT
            var service = new TaskReceiverService(_serviceBusHandler!.Object, _dataContext!, _mapper!);

            service.RegisterReceivers();

            // Assert
            _serviceBusHandler.Verify(v => v.RegisterMessageHandler<CreateTaskDto>(It.IsAny<string>(),
                It.IsAny<Func<CreateTaskDto?, Task>>()), Times.Once());

            _serviceBusHandler.Verify(v => v.RegisterMessageHandler<UpdateTaskDto>(It.IsAny<string>(),
                It.IsAny<Func<UpdateTaskDto?, Task>>()), Times.Once());
        }

        [TestMethod]
        public async Task CreatTaskAsync_CreateNewTask_OK()
        {
            // ACT
            var service = new TaskReceiverService(_serviceBusHandler!.Object, _dataContext!, _mapper!);

            var addMethod = typeof(TaskReceiverService).GetMethod("CreateAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var createArgs = new CreateTaskDto
            {
                Name = "TestName",
                Description = "TestDesc",
                Status = Status.Completed,
                AssignedTo = "AssignedTo"
            };

            addMethod!.Invoke(service, [createArgs]);

            // Assert
            var actual = await _dataContext!.Tasks.LastAsync();

            Assert.AreEqual(createArgs.Name, actual.Name);
            Assert.AreEqual(createArgs.Description, actual.Description);
            Assert.AreEqual(createArgs.Status, actual.Status);
            Assert.AreEqual(createArgs.AssignedTo, actual.AssignedTo);
        }

        [TestMethod]
        public async Task UpdateTaskAsync_AddAndUpdateTask_OK()
        {
            //Assert
            var newTask = new TaskEntity
            {
                Id = 2,
                Name = "TestName",
                Description = "TestDesc",
                Status = Status.NotStarted
            };
            _dataContext!.Tasks.Add(newTask);

            await _dataContext.SaveChangesAsync();

            // ACT
            var service = new TaskReceiverService(_serviceBusHandler!.Object, _dataContext, _mapper!);
            var addMethod = typeof(TaskReceiverService).GetMethod("UpdateAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var createArgs = new UpdateTaskDto
            {
                Id = newTask.Id,
                NewStatus = Status.Completed
            };

            addMethod!.Invoke(service, [createArgs]);

            // Assert
            var actual = await _dataContext.Tasks.SingleAsync(s => s.Id == createArgs.Id);
            Assert.AreEqual(createArgs.NewStatus, actual.Status);
        }

        [TestMethod]
        public async Task UpdateTaskAsync_NotExistingTask_NOK()
        {
            //Assert

            var newTask = new TaskEntity
            {
                Id = 3,
                Name = "TestName",
                Description = "TestDesc",
                Status = Status.NotStarted
            };
            _dataContext!.Tasks.Add(newTask);

            await _dataContext.SaveChangesAsync();

            // ACT
            var service = new TaskReceiverService(_serviceBusHandler!.Object, _dataContext, _mapper!);
            var addMethod = typeof(TaskReceiverService).GetMethod("UpdateAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var createArgs = new UpdateTaskDto
            {
                Id = 4,
                NewStatus = Status.Completed
            };

            addMethod!.Invoke(service, [createArgs]);

            var actual = await _dataContext.Tasks.FirstOrDefaultAsync(s => s.Id == createArgs.Id);
            Assert.IsNull(actual);
        }
    }
}
