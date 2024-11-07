How to start...

1. Open TaskManagementApi.sln in VisualStudio

2. Change ServiceBusConnection and TaskDbConnectionString in TaskManagementApi/appsettings.json and Receiver/appsettings.json
"ConnectionStrings": {
    "ServiceBusConnection": "Endpoint=sb://XXXXXXXXXXXXXXX.servicebus.windows.net",
    "TaskDbConnectionString": "Data Source=XXXXXXXXXXXXXXX;Initial Catalog=XXXXXXXXXXXXXX;"
  }
2.1 FYI tested on MsSql Express server. No need to create an empty db, the EF will do it for you.

3. Create 2 queues in Azure Service Bus with names: create-task-queue, update-task-queue 

4. In solution properties -> Multiple stratup projects -> Receiver and TaskManagementApi set to 'Start'

5. Run the solution
