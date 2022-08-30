using System;
using System.Collections.Generic;

namespace TABS.Data
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression, Dictionary<string, string> jobData)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            JobData = jobData;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
        public Dictionary<string, string> JobData { get; }
    }
}