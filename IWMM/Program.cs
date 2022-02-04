using CommandLine;
using IWMM.Core;
using IWMM.Entities;
using IWMM.Parameters;
using IWMM.Repositories;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Network;
using IWMM.Services.Impl.Traefik;
using IWMM.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickLogger.Extensions.NetCore;
using System.Reflection;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args)
{
    var loggerName = Assembly.GetExecutingAssembly()?.GetName()?.Name;

    CommandLineParams commandLineParams = new CommandLineParams();

    Parser.Default.ParseArguments<CommandLineParams>(args)
        .WithParsed(o =>
        {
            commandLineParams = o;
        });

    return Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((builderContext, config) =>
        {
            var env = builderContext.HostingEnvironment;

            config
                .AddYamlFile(commandLineParams.ConfigFile, optional: false, reloadOnChange: true);
        })
        .UseSystemd()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddQuickLogger();

            services.UseYamlSerializerDeserializer();

            services.Configure<MainSettings>(hostContext.Configuration.GetSection("MainSettings"));

            services.AddTransient<IFqdnResolver, FqdnResolver>();

            services.AddSingleton<IEntryRepository, LiteDbEntryRepository>();

            services.AddTransient<EntriesToTraefikSchemaAdaptor>();

            services.AddTransient<TraefikWhitelistYamlRepository>();

            services.AddTransient<Func<SchemaType, ISchemaRepository>>(f => s =>
            {
                switch (s)
                {
                    case SchemaType.TraefikIpWhitelistMiddlewareFile:
                        return f.GetService<TraefikWhitelistYamlRepository>();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(s), s, null);
                }
            });

            services.AddTransient<Func<SchemaType, IEntriesToSchemaAdaptor>>(f => s =>
            {
                switch (s)
                {
                    case SchemaType.TraefikIpWhitelistMiddlewareFile:
                        return f.GetService<EntriesToTraefikSchemaAdaptor>();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(s), s, null);
                }
            });

            services.AddHostedService<Worker>();
        });
}
