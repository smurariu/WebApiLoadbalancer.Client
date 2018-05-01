using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Monitoring;
using WebApi.Loadbalancer.Client;

namespace WebApiApp
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvcCore()
                    .AddJsonFormatters();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory
                .AddDebug()
                .AddConsole();
            
            applicationLifetime.UseLoadbalancer(5000, "http://localhost:2002");

            app.UseHealthChecks(() => Task.FromResult(new[]{
                new HealthCheck(dependencyName: "test dep", isDown: false, responseTimeMilliseconds: 100)}));
            
            app.UseMvc();
        }
    }
}
