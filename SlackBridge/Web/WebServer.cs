using System;
using System.Net;
using System.Text;
using System.Threading;

using Oxide.Ext.SlackBridge.Logging;

namespace Oxide.Ext.SlackBridge.Web
{
    public class WebServer
    {
        private HttpListener _httpListener;
        private Func<HttpListenerRequest, string> _callback;

        public WebServer(string url, Func<HttpListenerRequest, string> callback)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _callback = callback;
        }

        public void Start()
        {
            _httpListener.Start();
            Process();
        }

        public void Stop()
        {
            _httpListener.Stop();
            _httpListener.Close();
        }

        private void Process()
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                try
                {
                    while (_httpListener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((y) =>
                        {
                            var context = y as HttpListenerContext;

                            try
                            {
                                var responseString = _callback(context.Request);
                                var buffer = Encoding.UTF8.GetBytes(responseString);
                                context.Response.ContentLength64 = buffer.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("An error occurred: {0}\n{1}", e.Message, e.Data);
                            }
                            finally
                            {
                                context.Response.OutputStream.Close();
                            }
                        }, _httpListener.GetContext());
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("An error occurred: {0}\n{1}", e.Message, e.Data);
                }
            });
        }
    }
}
