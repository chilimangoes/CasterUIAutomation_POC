using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasterUIAutomation.Communication
{
    public interface IHttpAsyncHandler : IDisposable
    {
        /// <summary>
        /// Main execution method of the handler which returns an HTTP response intent.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        Task Execute(HttpRequestContext state);
    }
}
