﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    internal class StartupOverideTelemClient
    {
        private readonly Mock<IBotTelemetryClient> _telemClient;

        public StartupOverideTelemClient(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            _telemClient = new Mock<IBotTelemetryClient>();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            BotConfiguration.Load("testbot.bot", null);
            services.AddBotApplicationInsights(_telemClient.Object);

            // Adding IConfiguration in sample test server.  Otherwise this appears to be
            // registered.
            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBotApplicationInsights();
            var telemetryClient = app.ApplicationServices.GetService<IBotTelemetryClient>();
            Assert.NotNull(telemetryClient);
            Assert.Equal(telemetryClient, _telemClient.Object);
        }
    }
}
