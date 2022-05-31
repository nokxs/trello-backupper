using Quartz;
using Quartz.Impl;

namespace trello_backuper.docker
{
    public class Worker : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cron = Environment.GetEnvironmentVariable("CRON") ?? throw new ArgumentException("Please provide a valid cron expression. See readme for details.");

            var factory = new StdSchedulerFactory();
            
            var scheduler = await factory.GetScheduler(stoppingToken);
            await scheduler.Start(stoppingToken);

            var job = JobBuilder.Create<BackupJob>()
                .WithIdentity("myJob", "group")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger", "group")
                .StartNow()
                .WithCronSchedule(cron)
                .ForJob("myJob", "group")
                .Build();

            await scheduler.ScheduleJob(job, trigger, stoppingToken);
        }
    }
}