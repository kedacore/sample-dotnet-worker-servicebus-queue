using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Keda.Samples.Dotnet.API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var openApiInformation = new OpenApiInfo
            {
                Title = "KEDA API",
                Version = "v1"
            };

            services.AddScoped<QueueClient>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration["KEDA_SERVICEBUS_QUEUE_CONNECTIONSTRING"];
                var serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString);
                return new QueueClient(serviceBusConnectionStringBuilder.GetNamespaceConnectionString(), serviceBusConnectionStringBuilder.EntityPath, ReceiveMode.PeekLock);
            });

            services.AddSwaggerGen(swaggerGenerationOptions =>
            {
                swaggerGenerationOptions.SwaggerDoc("v1", openApiInformation);
                swaggerGenerationOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Keda.Samples.Dotnet.API.Open-Api.xml"));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint("v1/swagger.json", "Keda.Samples.Dotnet.API");
                swaggerUiOptions.DocumentTitle = "KEDA API";
            });
        }
    }
}