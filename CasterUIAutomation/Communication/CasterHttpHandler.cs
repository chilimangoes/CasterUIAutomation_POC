using CasterUIAutomation.Actions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CasterUIAutomation.Communication
{
    public class CasterHttpHandler : IHttpAsyncHandler
    {
        //ConcurrentQueue<IAction> actionQueue = new ConcurrentQueue<IAction>();

        Task IHttpAsyncHandler.Execute(HttpRequestContext context)
        {
            var url = context.Request.Url;
            string path = url.AbsolutePath.ToLower();
            string queryString = url.Query.StartsWith("?") ? url.Query.Remove(0, 1) : url.Query;
            string body;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                body = reader.ReadToEnd();
            }
            var parameters = ParseParameters(queryString, body);
            
            switch (path)
            {

                // TODO: refactor this to put actions on a queue and run asynchronously so we don't block the caller

                case "/key":
                    Debug.WriteLine("Handling 'Key' command:");
                    Debug.WriteLine("Query String: " + queryString);
                    Debug.WriteLine("Body: " + body);
                    var action = new KeyAction(parameters);
                    //actionQueue.Enqueue(action);
                    action.ParseSpec();
                    break;
                default:
                    string message = "Unrecognized commnand in URL: " + path;
                    Debug.WriteLine("ERROR: " + message);
                    // TODO: change this to return a proper HTTP response instead
                    throw new InvalidOperationException(message);
            }
            return Task.FromResult(true);
        }

        Dictionary<string, string> ParseParameters(string queryString, string body)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var item in HttpUtility.ParseQueryStringParameters(queryString))
            {
                parameters.Add(item.Key, item.Value);
            }
            foreach (var item in HttpUtility.ParseQueryStringParameters(body))
            {
                parameters.Add(item.Key, item.Value);
            }
            return parameters;
        }
    }

    

}
