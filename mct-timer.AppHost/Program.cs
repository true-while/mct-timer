var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.mct_timer>("mct-timer")
       .WithExternalHttpEndpoints();

builder.Build().Run();
