﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime.Extensions
{
    /// <summary>
    /// Extensions for setting up Runtime IConfiguration.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        private const string AppSettingsFileName = @"appsettings.json";
        private const string ComposerDialogsDirectoryName = "ComposerDialogs";
        private const string DevelopmentApplicationRoot = "./";
        private const string DialogFileExtension = ".dialog";

        /// <summary>
        /// Setup the provided <see cref="IConfigurationBuilder"/> with the required Runtime configuration.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        /// <param name="isDevelopment">Indicates whether the application environment is set to 'Development'.</param>
        /// <returns>
        /// Supplied <see cref="IConfigurationBuilder"/> instance with additional in-memory configuration provider.
        /// </returns>
        public static IConfigurationBuilder AddBotRuntimeConfiguration(
            this IConfigurationBuilder builder,
            string applicationRoot,
            bool isDevelopment)
        {
            return AddBotRuntimeConfiguration(
                builder,
                applicationRoot,
                settingsDirectory: null,
                isDevelopment: isDevelopment);
        }

        /// <summary>
        /// Setup the provided <see cref="IConfigurationBuilder"/> with the required Runtime configuration.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        /// <param name="settingsDirectory">
        /// The relative path to the directory containing the appsettings.json file to add as a configuration source.
        /// If null is specified, appsettings.json will be located within the application root directory.
        /// </param>
        /// <param name="isDevelopment">Indicates whether the application environment is set to 'Development'.</param>
        /// <returns>
        /// Supplied <see cref="IConfigurationBuilder"/> instance with additional in-memory configuration provider.
        /// </returns>
        public static IConfigurationBuilder AddBotRuntimeConfiguration(
            this IConfigurationBuilder builder,
            string applicationRoot,
            string settingsDirectory,
            bool isDevelopment)
        {
            // Use Composer bot path adapter
            builder.AddBotRuntimeProperties(
                applicationRoot: applicationRoot,
                isDevelopment: isDevelopment);

            IConfiguration configuration = builder.Build();

            string botRootPath = configuration.GetValue<string>(ConfigurationConstants.BotKey);
            string configFilePath = Path.GetFullPath(
                Path.Combine(botRootPath, settingsDirectory, AppSettingsFileName));

            builder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);

            // Use Composer luis and qna settings extensions
            builder.AddComposerConfiguration();

            return builder;
        }

        /// <summary>
        /// Provides a collection of in-memory configuration values for the bot runtime to
        /// the provided <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        /// <param name="isDevelopment">Indicates whether the application environment is set to 'Development'.</param>
        /// <returns>
        /// Supplied <see cref="IConfigurationBuilder"/> instance with additional in-memory configuration provider.
        /// </returns>
        private static IConfigurationBuilder AddBotRuntimeProperties(
            this IConfigurationBuilder builder,
            string applicationRoot,
            bool isDevelopment = true)
        {
            if (string.IsNullOrEmpty(applicationRoot))
            {
                throw new ArgumentNullException(nameof(applicationRoot));
            }

            applicationRoot = isDevelopment
                ? Path.GetFullPath(Path.Combine(applicationRoot, DevelopmentApplicationRoot))
                : applicationRoot;

            string botRoot = isDevelopment
                ? applicationRoot
                : Path.Combine(applicationRoot, ComposerDialogsDirectoryName);

            var settings = new Dictionary<string, string>
            {
                {
                    ConfigurationConstants.ApplicationRootKey,
                    applicationRoot
                },
                {
                    ConfigurationConstants.BotKey,
                    botRoot
                },
                {
                    ConfigurationConstants.DefaultRootDialogKey,
                    GetDefaultRootDialog(botRoot)
                }
            };

            builder.AddInMemoryCollection(settings);
            return builder;
        }

        /// <summary>
        /// Setup configuration to utilize the settings file generated by bf luis:build and qna:build. This is a luis
        /// and qnamaker settings extensions adapter aligning with Composer customized settings.
        /// </summary>
        /// <remarks>
        /// This will pick up LUIS_AUTHORING_REGION or --region settings as the setting to target.
        /// This will pick up --environment as the environment to target.  If environment is Development it will use
        /// the name of the logged in user.
        /// This will pick up --root as the root folder to run in.
        /// </remarks>
        /// <param name="builder">Configuration builder to modify.</param>
        /// <returns>Modified configuration builder.</returns>
        private static IConfigurationBuilder AddComposerConfiguration(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var botRoot = configuration.GetValue<string>("bot") ?? ".";
            var luisRegion = configuration.GetValue<string>("LUIS_AUTHORING_REGION")
                ?? configuration.GetValue<string>("luis:authoringRegion")
                ?? configuration.GetValue<string>("luis:region") ?? "westus";
            var qnaRegion = configuration.GetValue<string>("qna:qnaRegion") ?? "westus";
            var environment = configuration.GetValue<string>("luis:environment") ?? Environment.UserName;
            var settings = new Dictionary<string, string>();
            var luisEndpoint = configuration.GetValue<string>("luis:endpoint");
            if (string.IsNullOrWhiteSpace(luisEndpoint))
            {
                luisEndpoint = $"https://{luisRegion}.api.cognitive.microsoft.com";
            }

            settings["luis:endpoint"] = luisEndpoint;
            settings["BotRoot"] = botRoot;
            builder.AddInMemoryCollection(settings);
            if (environment == "Development")
            {
                environment = Environment.UserName;
            }

            var luisSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"luis.settings.{environment.ToLowerInvariant()}.{luisRegion}.json"));
            var luisSettingsFile = new FileInfo(luisSettingsPath);
            if (luisSettingsFile.Exists)
            {
                builder.AddJsonFile(luisSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            var qnaSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"qnamaker.settings.{environment.ToLowerInvariant()}.{qnaRegion}.json"));
            var qnaSettingsFile = new FileInfo(qnaSettingsPath);
            if (qnaSettingsFile.Exists)
            {
                builder.AddJsonFile(qnaSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            return builder;
        }

        private static string GetDefaultRootDialog(string botRoot)
        {
            var directory = new DirectoryInfo(botRoot);
            foreach (FileInfo file in directory.GetFiles())
            {
                if (string.Equals(DialogFileExtension, file.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    return file.Name;
                }
            }

            return null;
        }
    }
}
