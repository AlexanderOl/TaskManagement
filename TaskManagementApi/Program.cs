using TaskManagementApi;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.ConfigureLogging(options =>
                   {
                       options.AddConsole();
                   });
                   webBuilder.UseStartup<Startup>();
               });
