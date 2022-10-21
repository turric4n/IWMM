using CommandLine;
using IWMM.Core;
using IWMM.Entities;
using IWMM.Parameters;
using IWMM.Repositories;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Network;
using IWMM.Services.Impl.Traefik;
using IWMM.Settings;
using System.Reflection;
using Serilog;

var loggerName = Assembly.GetExecutingAssembly()?.GetName()?.Name;

CommandLineParams commandLineParams = new CommandLineParams();

Parser.Default.ParseArguments<CommandLineParams>(args)
    .WithParsed(o =>
    {
        commandLineParams = o;
    });


Host.CreateDefaultBuilder();

var builder = WebApplication.CreateBuilder();

builder.WebHost

    .ConfigureAppConfiguration((builderContext, config) =>
    {
        config
            .AddYamlFile(commandLineParams.ConfigFile, optional: false, reloadOnChange: true);
    })

    .ConfigureServices((hostContext, services) =>
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        services.AddControllers();

        services.UseYamlSerializerDeserializer();

        services.Configure<MainSettings>(hostContext.Configuration.GetSection("MainSettings"));

        services.AddTransient<IFqdnResolver, FqdnResolver>();

        services.AddSingleton<IEntryRepository, LiteDbEntryRepository>();

        services.AddTransient<EntriesToTraefikSchemaAdaptor>();

        services.AddTransient<YamlRepository>();

        services.AddTransient<ISchemaMerger, SchemaMerger>();

        services.AddTransient<Func<SchemaType, ISchemaRepository>>(f => s =>
        {
            switch (s)
            {
                case SchemaType.TraefikIpWhitelistMiddlewareFile:
                    return f.GetService<YamlRepository>();
                case SchemaType.TraefikPlain:
                    return f.GetService<YamlRepository>();
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

var app = builder
    .Build();

app.MapControllers();

app.Run();
