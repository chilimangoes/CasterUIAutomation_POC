using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CasterUIAutomation.Communication
{

    // Adapted from https://github.com/JamesDunne/aardwolf

    public enum HttpAsyncHostStatus
    {
        Uninitialised,
        Initialised,
        Starting,
        Running,
        //Error,
    }

    public sealed class HttpAsyncHost
    {
        bool _stop = false;
        HttpListener _listener;
        IHttpAsyncHandler _handler;
        //HostContext _hostContext;
        //ConfigurationDictionary _configValues;
        // see here for tuning of the _accepts parameter:https://stackoverflow.com/questions/11167183/multi-threaded-httplistener-with-await-async-and-tasks
        readonly int _accepts;

        /// <summary>
        /// Creates an asynchronous HTTP host.
        /// </summary>
        /// <param name="handler">Handler to serve requests with</param>
        /// <param name="accepts">
        /// Higher values mean more connections can be maintained yet at a much slower average response time; fewer connections will be rejected.
        /// Lower values mean less connections can be maintained yet at a much faster average response time; more connections will be rejected.
        /// </param>
        public HttpAsyncHost(IHttpAsyncHandler handler, int accepts = 1)
        {
            SetStatus(HttpAsyncHostStatus.Uninitialised);

            _handler = handler ?? throw new ArgumentNullException("handler");
            _accepts = accepts;

            SetStatus(HttpAsyncHostStatus.Initialised);
        }


        public delegate void StatusChangeEvent(HttpAsyncHostStatus oldStatus, HttpAsyncHostStatus newStatus);
        public event StatusChangeEvent StatusChanged;

        public HttpAsyncHostStatus Status { get; private set; }

        private void SetStatus(HttpAsyncHostStatus newStatus)
        {
            HttpAsyncHostStatus oldStatus = Status;
            Status = newStatus;
            StatusChanged?.Invoke(oldStatus, newStatus);
        }

        public Task StartAsync(string hostName, int listenPort)
        {
            if (Status == HttpAsyncHostStatus.Uninitialised)
                throw new InvalidOperationException("Host is in an invalid state. Initialization may have failed.");
            if (_listener != null)
                throw new InvalidOperationException("Host has already been started.");

            SetStatus(HttpAsyncHostStatus.Starting);

            // Establish a host-handler context:
            //_hostContext = new HostContext(this, _handler);

            _listener = new HttpListener
            {
                IgnoreWriteExceptions = true
            };

            _listener.Prefixes.Add(string.Format("http://{0}:{1}/", hostName, listenPort));

            return Task.Run(() =>
            {
                // Configure the handler:
                //if (_configValues != null)
                //{
                //    var config = _handler as IConfigurationTrait;
                //    if (config != null)
                //    {
                //        var task = config.Configure(_hostContext, _configValues);
                //        if (task != null)
                //            if (!await task) return;
                //    }
                //}

                // Initialize the handler:
                //var init = _handler as IInitializationTrait;
                //if (init != null)
                //{
                //    var task = init.Initialize(_hostContext);
                //    if (task != null)
                //        if (!await task) return;
                //}

                try
                {
                    _stop = false;
                    // Start the HTTP listener:
                    _listener.Start();
                }
                catch (HttpListenerException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return;
                }

                // Accept connections:
                // Higher values mean more connections can be maintained yet at a much slower average response time; fewer connections will be rejected.
                // Lower values mean less connections can be maintained yet at a much faster average response time; more connections will be rejected.
                var semaphore = new Semaphore(_accepts, _accepts);

                SetStatus(HttpAsyncHostStatus.Running);

                while (!_stop)
                {
                    semaphore.WaitOne();
                    
#pragma warning disable 4014
                    _listener?.GetContextAsync().ContinueWith(async (t) =>
                    {
                        string errMessage;

                        try
                        {
                            semaphore.Release();

                            var context = await t;
                            await ProcessListenerContext(context, this);
                            return;
                        }
                        catch (Exception ex)
                        {
                            errMessage = ex.ToString();
                        }

                        await Console.Error.WriteLineAsync(errMessage);
                    });
#pragma warning restore 4014
                }

                SetStatus(HttpAsyncHostStatus.Initialised);
            });
        }

        public void Stop()
        {
            // TODO: thread safety
            _stop = true;
            _listener?.Stop();
            _listener?.Close();
            _handler?.Dispose();
            _listener = null;
            SetStatus(HttpAsyncHostStatus.Initialised);
        }
        
        static async Task ProcessListenerContext(HttpListenerContext listenerContext, HttpAsyncHost host)
        {
            Debug.Assert(listenerContext != null);

            try
            {
                // Get the response action to take:
                var requestContext = new HttpRequestContext(host, listenerContext);
                await host._handler.Execute(requestContext);
                
                // Close the response and send it to the client:
                listenerContext.Response.Close();
            }
            catch (HttpListenerException)
            {
                // Ignored.
            }
            catch (Exception ex)
            {
                // TODO: better exception handling. we should probably return an internal server error HTTP code.
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}
