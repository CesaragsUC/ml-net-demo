using Microsoft.Extensions.Hosting;
using MLImoveisCuritiba.TrainJob;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

// Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("TrainModelJob");

q.AddJob<TrainModelJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("TrainModelJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromHours(24))
            .RepeatForever()
        )
    );
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();

