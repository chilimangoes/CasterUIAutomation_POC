using System.Net;
using System.Security.Principal;

namespace CasterUIAutomation.Communication
{
    public sealed class HttpRequestContext
    {
        public HttpAsyncHost HostContext { get; private set; }

        public HttpListenerRequest Request { get; private set; }
        public HttpListenerResponse Response { get; private set; }
        //public IPrincipal User { get; private set; }

        public HttpRequestContext(HttpAsyncHost hostContext, HttpListenerContext listenerContext)
        {
            HostContext = hostContext;
            Request = listenerContext.Request;
            Response = listenerContext.Response;
            //User = listenerContext.User;
        }
    }
}
