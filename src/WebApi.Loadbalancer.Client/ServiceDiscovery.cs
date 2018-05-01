using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi.Loadbalancer.Client
{
    public class Registration
    {
        internal static Timer _timer = new Timer(60 * 1000); //60 seconds

        internal static void RegisterService(string port, string loadbalancerUrl, string registerEndpoint = "/register")
        {
            using (var client = new HttpClient())
            {
                var url = BuildRequestUri(loadbalancerUrl, registerEndpoint, port);
                Task<HttpResponseMessage> task = client.PostAsync(url, null);
                Task.WaitAll(task);
            }
        }

        internal static void UnregisterService(string port, string loadbalancerUrl, string unregisterEndpoint = "/unregister")
        {
            using (var client = new HttpClient())
            {
                var url = BuildRequestUri(loadbalancerUrl, unregisterEndpoint, port);
                Task<HttpResponseMessage> task = client.PostAsync(url, null);
                Task.WaitAll(task);
            }

            _timer.Stop();
            _timer.Dispose();
        }

        internal static void StartTimer(Action registrationAction)
        {
            _timer.Elapsed += (sender, e) => registrationAction();
            _timer.Start();
        }

        private static string BuildRequestUri(string loadbalancerUrl, string endpoint, string port)
        {
            // yes folks, this is 2018 and you have to do all of this to add
            // a simple query string parameter to a uri :|
            var uriBuilder = new UriBuilder(loadbalancerUrl);
            uriBuilder.Path = endpoint;
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["port"] = port.ToString();
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
    }

    public static class ServiceDisoveryExtensions
    {

        /// <summary>
        ///     Registers this web api with a load balancer
        /// </summary>
        /// <param name="applicationLifetime">The IApplicationLifetime parameter</param>
        /// <param name="port">The port the current web api is listening on</param>
        /// <param name="loadbalancerUrl">The url of the load balancer to register with</param>
        /// <param name="registerEndpoint">The register endpoint (defaults to "/register")</param>
        /// <param name="unregisterEndpoint">The unregister endpoint (defaults to "/unregister")</param>
        /// <returns></returns>
        public static IApplicationLifetime UseLoadbalancer(this IApplicationLifetime applicationLifetime, int port, string loadbalancerUrl, string registerEndpoint = "/register", string unregisterEndpoint = "/unregister")
        {
            Action registrationAction = () => Registration.RegisterService(port.ToString(), loadbalancerUrl, registerEndpoint);
            Action unregistrationAction = () => Registration.UnregisterService(port.ToString(), loadbalancerUrl, unregisterEndpoint);

            applicationLifetime.ApplicationStarted.Register(registrationAction);
            applicationLifetime.ApplicationStopping.Register(unregistrationAction);

            Registration.StartTimer(registrationAction);

            return applicationLifetime;
        }
    }
}