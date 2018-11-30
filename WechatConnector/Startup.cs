using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NLog.Web;
using Senparc.CO2NET;
using Senparc.Weixin.Entities;
using WeyhdBot.Core.Devices;
using WeyhdBot.Core.Devices.Options;
using WeyhdBot.MongoDb;
using WeyhdBot.MongoDb.Devices;
using WeyhdBot.MongoDb.Extensions;
using WeyhdBot.Wechat.Client;
using WeyhdBot.Wechat.Connector;
using WeyhdBot.Wechat.Extensions;
using WeyhdBot.Wechat.Options;

namespace WechatConnector
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            HostingEnvironment = env;
            Configuration = configuration;
        }

        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                //options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();//使用本地缓存必须添加
            services.AddSession();//使用Session

            ConfigureOptions(services);

            services.AddSharRepositoryFactory(Configuration);
            services.AddSenparcServers(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddSingleton<IMongoDBConnector, MongoDBConnector>();
            services.AddScoped<IWechatClient, WechatClient>();
            services.AddScoped<IDirectLineConnector, DirectLineConnector>();
            services.AddScoped<IDeviceRegistrar, MongoDBDeviceRegistrar>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.RegisterWeChat(env, senparcSetting, senparcWeixinSetting);
            loggerFactory.AddNLog();

            var nlogConfig = "nlog.config";
            var nlogEnvSpecific = "nlog." + env.EnvironmentName + ".config";
            if (File.Exists(nlogEnvSpecific))
            {
                nlogConfig = nlogEnvSpecific;
            }

            //needed for non-NETSTANDARD platforms: configure nlog.config in your project root
            env.ConfigureNLog(nlogConfig);
            bool isRunningInAzureWebApp = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")) && !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
            if (isRunningInAzureWebApp)
            {
                string homeDir = Environment.GetEnvironmentVariable("HOME");
                string logFilesRoot = Path.Combine(homeDir, "LogFiles", "NLog");
                NLog.LogManager.Configuration.Variables["logroot"] = logFilesRoot;
            }
            app.UseResponseCompression();
            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<DirectLineConnectorOptions>(Configuration.GetSection("DirectLineConnectorOptions"));
            services.Configure<DeviceRegistrationOptions>(Configuration.GetSection("DeviceRegistrationOptions"));
        }
    }
}
