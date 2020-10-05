using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnycastHealthChecker
{
    public interface INginxHelathCheck
    {
      

        Task<bool> IsHealthy();
    }
}