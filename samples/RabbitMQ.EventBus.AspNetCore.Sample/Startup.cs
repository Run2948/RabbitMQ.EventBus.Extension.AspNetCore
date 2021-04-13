using System;
using System.Reflection;
using AspNetCore.WebSample.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.EventBus.Extension.AspNetCore;

namespace AspNetCore.WebSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRabbitMQEventBus(() => Configuration.GetConnectionString("Rabbit"),
                eventBusOption =>
                {
                    eventBusOption.ClientProvidedAssembly(typeof(Startup).Namespace);
                    eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
                    eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
                    eventBusOption.MessageTTL(2000);
                    eventBusOption.SetBasicQos(10);
                    eventBusOption.DeadLetterExchangeConfig(config =>
                    {
                        config.Enabled = true;
                        config.ExchangeNameSuffix = null;
                    });
                });

            services.AddControllers();
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IRabbitMQEventBus eventBus)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // app.UseRabbitMQEventBus();

            eventBus.Subscribe<MessageBody>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });
        }
    }
}
