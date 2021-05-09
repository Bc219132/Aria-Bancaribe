// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.6.2

using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BanCoreBot.Dialogs;
using BanCoreBot.Infrastructure;
using BanCoreBot.Infrastructure.Luis;
using BanCoreBot.Infrastructure.QnAMakerAI;
using BanCoreBot.Infrastructure.SendGrid;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace BanCoreBot
{
    public class Startup
    {
       /* public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }*/

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {

            // Integracion con KV
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                     }
            };
            var client = new SecretClient(new Uri("https://bancabotdesakv.vault.azure.net/"), new DefaultAzureCredential(), options);
            //var client = new SecretClient(new Uri("https://bancabotkv.vault.azure.net/"), new DefaultAzureCredential(), options);
            
            KeyVaultSecret secret = client.GetSecret("TwitterOptions");
            string jsonKV = secret.Value;
           
            var streamKV = new MemoryStream(Encoding.ASCII.GetBytes(jsonKV));

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonStream(streamKV)
                .AddEnvironmentVariables()
                .AddConfiguration(configuration);

            Configuration = builder.Build();


            EasyAccessToConfig.Configuration = configuration;
        }




        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTwitterAdapter(x => Configuration.Bind("TwitterOptions", x));

            var storage = new AzureBlobStorage(
                    Configuration.GetSection("StorageConnectionString").Value,
                    Configuration.GetSection("StorageContainer").Value
                );
            
            var userState = new UserState(storage);
            services.AddSingleton(userState);
            

            var privateconversationState = new PrivateConversationState(storage);
            services.AddSingleton(privateconversationState);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<ILuisService, LuisService>();
            services.AddSingleton<IQnAMakerAIService, QnAMakerAIService>();
            services.AddSingleton<ISendGridEmailService, SendGridEmailService>();
            services.AddTransient<RootDialog>();

            services.AddTransient<IBot, BanCoreBot<RootDialog>>();


            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();

            // Add the HttpClientFactory to be used for the API´s Calls.


            var consumerKey = Configuration.GetSection("ApiAuthConsumerKey").Value;
            var consumerSecret = Configuration.GetSection("ApiAuthConsumerSecret").Value;
            var encodedToken = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("utf-8")
                                  .GetBytes(consumerKey + ":" + consumerSecret));

            services.AddHttpClient("AuthenticationAPI",client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("ApiAuthBaseUrl").Value + "/token") ;
                client.DefaultRequestHeaders.Add("Authorization", encodedToken);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(1))
            .AddPolicyHandler(GetRetryPolicy()); 

            
            services.AddHttpClient("QueryClaim", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("ApiConsultaReclamoUrl").Value);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(1))
            .AddPolicyHandler(GetRetryPolicy());

            // Add the HttpClientFactory to be used for the QnAMaker calls.
            services.AddHttpClient();
            // Create the telemetry client.
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

            // Add telemetry initializer that will set the correlation context for all telemetry items.
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();

            // Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();

            // Create the telemetry middleware to initialize telemetry gathering
            services.AddSingleton<TelemetryInitializerMiddleware>();
            /*services.AddSingleton<TelemetryInitializerMiddleware>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var loggerMiddleware = sp.GetService<TelemetryLoggerMiddleware>();
                return new TelemetryInitializerMiddleware(httpContextAccessor, loggerMiddleware, logActivityTelemetry: true);
            }); */

            // Create the telemetry middleware (used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            /*services.AddSingleton<TelemetryLoggerMiddleware>(sp =>
            {
                var telemetryClient = sp.GetService<IBotTelemetryClient>();
                
                return new TelemetryLoggerMiddleware(telemetryClient, logPersonalInformation: true);
            });*/
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseTwitterAdapter();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
