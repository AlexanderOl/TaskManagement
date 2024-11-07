using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Common.Interfaces;
using Receiver;
using Common.Constants;
using Microsoft.Azure.ServiceBus;


var builder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


IConfiguration configuration = builder.Build();

var serviceProvider = new ServiceCollection()
    .AddDbContext<DataContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("TaskDbConnectionString")))
    .AddSingleton<TaskReceiverService>()
    .AddTransient<IServiceBusHandler, ServiceBusHandler>()
    .AddSingleton<IQueueClient>(provider =>
                new QueueClient(configuration.GetConnectionString("ServiceBusConnection"),
                    ServiceBusConstants.UpdateTaskQueue))
    .AddSingleton<IQueueClient>(provider =>
    new QueueClient(configuration.GetConnectionString("ServiceBusConnection"),
        ServiceBusConstants.CreateTaskQueue))
    .AddSingleton<IConfiguration>(configuration)
    .AddAutoMapper(typeof(MappingProfile))
    .AddLogging()
    .BuildServiceProvider();

using var taskService = serviceProvider.GetRequiredService<TaskReceiverService>();
taskService.RegisterReceivers();

Console.ReadLine();
