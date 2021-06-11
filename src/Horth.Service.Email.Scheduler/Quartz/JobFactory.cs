﻿using Microsoft.Extensions.DependencyInjection;
using OneOffice.Server.Scheduler.Services;
using Quartz;
using Quartz.Spi;
using System;

public class SingletonJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    public SingletonJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return _serviceProvider.GetRequiredService<QuartzJobRunner>();
    }

    public void ReturnJob(IJob job) { }
}