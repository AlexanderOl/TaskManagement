using Azure.Messaging.ServiceBus;
using Common.Constants;
using Common.Interfaces;
using Common.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Extensions;
using TaskManagementApi.Services;

namespace TaskManagementApi
{
    public class Startup(IConfiguration configuration)
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<TaskSenderService>();
            services.AddTransient<IServiceBusHandler, ServiceBusHandler>();
            services.AddSingleton<IQueueClient>(provider =>
                new QueueClient(configuration.GetConnectionString("ServiceBusConnection"), 
                    ServiceBusConstants.UpdateTaskQueue));
            services.AddSingleton<IQueueClient>(provider =>
                new QueueClient(configuration.GetConnectionString("ServiceBusConnection"),
                    ServiceBusConstants.CreateTaskQueue));

            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("TaskDbConnectionString")));

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddMemoryCache();

            services.AddAutoMapper(typeof(MappingProfile));

            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            context.Database.EnsureCreated();
        }
    }
}