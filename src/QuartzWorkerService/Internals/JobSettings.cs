using System.Collections.Generic;

namespace Farakav.QuartzWorkerService.Internals
{
    internal class JobSettings
    {
        public List<JobCronExpression> Jobs { get; set; }
    }
}