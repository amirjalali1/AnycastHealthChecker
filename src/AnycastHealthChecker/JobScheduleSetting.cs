using System;
using System.Collections.Generic;
using System.Text;

namespace AnycastHealthChecker
{
    public  class JobScheduleSetting
    {
        public JobScheduleSetting(Type jobType, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
    }
}
