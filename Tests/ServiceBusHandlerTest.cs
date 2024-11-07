using Common.Constants;
using Common.Dtos;
using Common.Enums;
using Common.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Text.Json;

namespace Tests
{
    [TestClass]
    public class ServiceBusHandlerTest
    {
        private Mock<IConfiguration>? _mockConfiguration;
        private Mock<ILogger<ServiceBusHandler>>? _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(config => config["ConnectionStrings:ServiceBusConnection"])
                .Returns(string.Empty);

            _mockConfiguration.Setup(config => config["MaxRetryCount"])
                .Returns("1");

            _logger = new Mock<ILogger<ServiceBusHandler>>();
        }

        [TestMethod]
        public async Task SendMessageAsync_SendToEachQueue_OK()
        {
            //Arrange
            var queueClient1 = new Mock<IQueueClient>();
            var queueClient2 = new Mock<IQueueClient>();

            queueClient1.Setup(service => service.QueueName).Returns(ServiceBusConstants.CreateTaskQueue);
            queueClient1.Setup(service => service.SendAsync(It.IsAny<Message>()));
            queueClient2.Setup(service => service.QueueName).Returns(ServiceBusConstants.UpdateTaskQueue);
            queueClient2.Setup(service => service.SendAsync(It.IsAny<Message>()));

            IEnumerable<IQueueClient> myServices =
            [
                queueClient1.Object,
                queueClient2.Object
            ];

            // ACT
            var service = new ServiceBusHandler(_mockConfiguration!.Object, myServices, _logger!.Object);

            var createArgs = new CreateTaskDto
            {
                Name = "TestName",
                Description = "TestDesc",
                Status = Status.Completed
            };

            await service.SendMessageAsync(createArgs, ServiceBusConstants.CreateTaskQueue);

            var updateArgs = new UpdateTaskDto { Id = 1, NewStatus = Status.Completed };

            await service.SendMessageAsync(updateArgs, ServiceBusConstants.UpdateTaskQueue);

            //Assert

            queueClient1.Verify(service => service.SendAsync(It.IsAny<Message>()), Times.Once());
            queueClient2.Verify(service => service.SendAsync(It.IsAny<Message>()), Times.Once());

        }

        [TestMethod]
        public void SendMessageAsync_MaxRetryExceeded_ServiceBusException()
        {
            //Arrange
            var maxRetryCount = 3;

            _mockConfiguration!.Setup(config => config["MaxRetryCount"])
              .Returns(maxRetryCount.ToString());

            var queueClient1 = new Mock<IQueueClient>();

            queueClient1.Setup(service => service.QueueName).Returns(ServiceBusConstants.CreateTaskQueue);
            queueClient1.Setup(service => service.SendAsync(It.IsAny<Message>()))
                .Throws(new ServiceBusException(false));

            IEnumerable<IQueueClient> myServices =
            [
                queueClient1.Object
            ];

            //Act

            var service = new ServiceBusHandler(_mockConfiguration.Object, myServices, _logger!.Object);

            var createArgs = new CreateTaskDto
            {
                Name = "TestName",
                Description = "TestDesc",
                Status = Status.Completed
            };

            //Assert
            Assert.ThrowsExceptionAsync<ServiceBusException>(() =>
                service.SendMessageAsync(createArgs, ServiceBusConstants.CreateTaskQueue));

            queueClient1.Verify(service => service.SendAsync(It.IsAny<Message>()), Times.Exactly(maxRetryCount));

        }


        [TestMethod]
        public void RegisterMessageHandler_RegisterOneQueue_OK()
        {
            //Arrange
            var queueClient1 = new Mock<IQueueClient>();

            queueClient1.Setup(service => service.QueueName).Returns(ServiceBusConstants.CreateTaskQueue);
            queueClient1.Setup(service => service.CompleteAsync(It.IsAny<string>()));

            Func<Message, CancellationToken, Task>? capturedCallback = null;
            queueClient1.Setup(q => q.RegisterMessageHandler(It.IsAny<Func<Message, CancellationToken, Task>>(),
                                                                It.IsAny<MessageHandlerOptions>()))
                           .Callback<Func<Message, CancellationToken, Task>, MessageHandlerOptions>((callback, options) =>
                           {
                               capturedCallback = callback;
                           });

            IEnumerable<IQueueClient> myServices =
            [
                queueClient1.Object
            ];

            var mockFunc = new Mock<Func<CreateTaskDto?, Task>>();

            //Act
            var service = new ServiceBusHandler(_mockConfiguration!.Object, myServices, _logger!.Object);
            service.RegisterMessageHandler(ServiceBusConstants.CreateTaskQueue, mockFunc.Object);

            //Assert

            Assert.IsNotNull(capturedCallback);

            queueClient1.Verify(f => f.RegisterMessageHandler(
                It.IsAny<Func<Message, CancellationToken, Task>>(),
                It.IsAny<MessageHandlerOptions>()), Times.Once());
        }

        [TestMethod]
        public async Task RegisterMessageHandler_RegisterAndReceiveMsg_OK()
        {
            //Arrange
            var queueClient1 = new Mock<IQueueClient>();

            queueClient1.Setup(service => service.QueueName).Returns(ServiceBusConstants.CreateTaskQueue);
            queueClient1.Setup(service => service.CompleteAsync(It.IsAny<string>()));

            Func<Message, CancellationToken, Task>? capturedCallback = null;
            queueClient1.Setup(q => q.RegisterMessageHandler(It.IsAny<Func<Message, CancellationToken, Task>>(),
                                                                It.IsAny<MessageHandlerOptions>()))
                           .Callback<Func<Message, CancellationToken, Task>, MessageHandlerOptions>((callback, options) =>
                           {
                               capturedCallback = callback;
                           });

            IEnumerable<IQueueClient> myServices =
            [
                queueClient1.Object
            ];

            var mockFunc = new Mock<Func<CreateTaskDto?, Task>>();
            var testDto = new CreateTaskDto
            {
                Name = "Test Task",
                Description = "This is a test task description",
                Status = Status.Completed
            };

            var serializedDto = JsonSerializer.SerializeToUtf8Bytes(testDto);

            var message = new Message(serializedDto);
            var systemProperties = message.SystemProperties;
            var type = systemProperties.GetType();
            var lockToken = Guid.NewGuid();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            type.GetMethod("set_LockTokenGuid", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(systemProperties, [lockToken]);
            type.GetMethod("set_SequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(systemProperties, [0]);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            //Act

            var service = new ServiceBusHandler(_mockConfiguration!.Object, myServices, _logger!.Object);
            service.RegisterMessageHandler(ServiceBusConstants.CreateTaskQueue, mockFunc.Object);

            Assert.IsNotNull(capturedCallback);
            await capturedCallback(message, CancellationToken.None);

            //Assert

            queueClient1.Verify(f => f.RegisterMessageHandler(
                It.IsAny<Func<Message, CancellationToken, Task>>(),
                It.IsAny<MessageHandlerOptions>()), Times.Once());

            mockFunc.Verify(v => v(It.IsAny<CreateTaskDto?>()), Times.Once);

        }
    }
}
