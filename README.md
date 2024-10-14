# IWMM (IP Whitelist Middleware Manager)

## Useful tool to manage FQDN whitelisting rules for multiple webservers

## Dependencies

The project uses the following dependencies:

- Serilog for logging
- Controllers for handling HTTP requests
- YAML serializer/deserializer

## Configuration

The application settings are configured in the `MainSettings` section of the configuration file.

```csharp
services.Configure<MainSettings>(hostContext.Configuration.GetSection("MainSettings"));
```

# Configuration Documentation

This document provides an overview of the configuration settings used in the `iwmm.yml` file.

## MainSettings

- **FqdnUpdateJobSeconds**: Interval in seconds for updating the FQDN (Fully Qualified Domain Name). Default is `60`.
- **UseLdap**: Boolean flag to enable or disable LDAP usage. Default is `true`.
- **LdapUpdateJobSeconds**: Interval in seconds for updating LDAP. Default is `60`.
- **BaseLdapUri**: The base URI for the LDAP server. Example: `ldap://ldapserver:389/`.
- **LdapScavengeScope**: The scope for LDAP scavenging. Example: `OU=*,OU=*,DC=domain,DC=local`.
- **AdditionalTraefikPlainFileSettingsPaths**: List of additional paths for Traefik plain file settings. Example: `["*.yml"]`.

## IpWhiteListSettings

This section defines the IP whitelist settings for Traefik middleware.

- **SchemaType**: The schema type for the IP whitelist middleware. Example: `TraefikIpWhitelistMiddlewareFile`.
- **TraefikMiddlewareSettings**:
  - **Name**: The name of the Traefik middleware setting. Example: `test1TestAllowed`.
  - **FilePath**: The file path for the Traefik middleware setting. Example: `test1Allowed.yml`.
- **AllowedEntries**: List of entries that are allowed. Example: `["Test1"]`.
- **ExcludedEntries**: List of entries that are excluded. Example: `["Balancer1"]`.

### Example Entries

1. **Entry 1**:
   - **SchemaType**: `TraefikIpWhitelistMiddlewareFile`
   - **TraefikMiddlewareSettings**:
     - **Name**: `test1TestAllowed`
     - **FilePath**: `test1Allowed.yml`
   - **AllowedEntries**: `["Test1"]`
   - **ExcludedEntries**: `["Balancer1"]`

2. **Entry 2**:
   - **SchemaType**: `TraefikIpWhitelistMiddlewareFile`
   - **TraefikMiddlewareSettings**:
     - **Name**: `test2TestAllowed`
     - **FilePath**: `test2Allowed.yml`
   - **AllowedEntries**: `["Test1", "Office"]`
   - **ExcludedEntries**: `["Balancer1"]`

3. **Entry 3**:
   - **SchemaType**: `TraefikIpWhitelistMiddlewareFile`
   - **TraefikMiddlewareSettings**:
     - **Name**: `test2GroupTestAllowed`
     - **FilePath**: `test2Allowed.yml`
   - **AllowedEntries**: `["AllDevelopers"]`
   - **ExcludedEntries**: `["Balancer1"]`

## Entries

- **Name**: The name of the entry. Example: `Test1`.
- **Fqdn**: The fully qualified domain name for the entry. Example: `test.com`.

## LdapJob

The `LdapJob` class is responsible for executing the LDAP discovery job at specified intervals. It inherits from the `BaseJob` class and utilizes various services to perform its tasks.

### Dependencies

The `LdapJob` class depends on the following services:
- `ILogger<LdapJob>`: For logging information.
- `IOptions<MainSettings>`: For accessing configuration settings.
- `ISettingsToSchemaFacade`: For updating LDAP entries and saving them into the repository.

### Constructor

The constructor initializes the `LdapJob` with the required dependencies.

public LdapJob(ILogger<LdapJob> logger, IOptions<MainSettings> options, ISettingsToSchemaFacade settingsToSchemaFacade) : base(logger, options)
{
    _settingsToSchemaFacade = settingsToSchemaFacade;
}

## SchemaType Enum

The `SchemaType` enum is defined in the `IWMM.Settings` namespace and is used to specify different types of schemas for the application. This enum includes the following values:

### Values

- `TraefikIpWhitelistMiddlewareFile`: Represents the schema type for Traefik IP Whitelist Middleware File.
- `TraefikPlain`: Represents the plain schema type for Traefik.
- `OpnSense`: Represents the schema type for OpnSense.

### Definition

The `SchemaType` enum is defined as follows:

namespace IWMM.Settings
{
    public enum SchemaType
    {
        TraefikIpWhitelistMiddlewareFile,
        TraefikPlain,
        OpnSense
    }
}

## OpnSenseIpWhiteListSettings

The `OpnSenseIpWhiteListSettings` class is used to configure the IP whitelist settings for OpnSense. It includes properties for allowed and excluded entries, as well as the schema type.

### Properties

- `SchemaType SchemaType`: Specifies the schema type for the settings.
- `List<string> AllowedEntries`: A list of IP addresses that are allowed.
- `List<string> ExcludedEntries`: A list of IP addresses that are excluded.

### Constructor

The constructor initializes the `AllowedEntries` and `ExcludedEntries` properties.

public OpnSenseIpWhiteListSettings()
{
    AllowedEntries = new List<string>();
    ExcludedEntries = new List<string>();
}

## TraefikIpWhiteListSettings

The `TraefikIpWhiteListSettings` class is used to configure the IP whitelist settings for Traefik middleware. It includes properties for allowed and excluded entries, as well as middleware-specific settings.

### Properties

- `SchemaType SchemaType`: Specifies the schema type for the settings.
- `TraefikMiddlewareSettings TraefikMiddlewareSettings`: Contains the middleware-specific settings.
- `List<string> AllowedEntries`: A list of IP addresses that are allowed.
- `List<string> ExcludedEntries`: A list of IP addresses that are excluded.

### Constructor

The constructor initializes the `AllowedEntries`, `ExcludedEntries`, and `TraefikMiddlewareSettings` properties.

public TraefikIpWhiteListSettings()
{
    AllowedEntries = new List<string>();
    ExcludedEntries = new List<string>();
    TraefikMiddlewareSettings = new TraefikMiddlewareSettings();
}

## PlainController

The `PlainController` class provides endpoints to retrieve LDAP information based on distinguished names (DN) and computer names. It interacts with an entry repository to fetch and process the required data.

### Endpoints

#### `GetLdapOu`

This method retrieves LDAP organizational unit (OU) information based on a distinguished name (DN).

- **URL**: `/ldapOu`
- **Method**: `GET`
- **Parameters**:
  - `dn` (string): The distinguished name, with semicolons (`;`) replaced by commas (`,`).
- **Returns**: A string containing all unique IP addresses associated with the DN, separated by new lines.

##### Example

```http
GET /ldapOu?dn=example;dn
```

## TraefikController

The `TraefikController` class provides endpoints to manage and retrieve Traefik-related configurations. It interacts with various services and repositories to perform its tasks.

### Dependencies

The `TraefikController` class depends on the following services:
- `ISettingsToSchemaFacade`: For updating settings and schemas.
- `IOptionsSnapshot<MainSettings>`: For accessing configuration settings.
- `ILogger<TraefikController>`: For logging information.
- `Func<SchemaType, ISchemaRepository>`: For locating schema repositories.
- `Func<SchemaType, IEntriesToSchemaAdaptor>`: For locating schema adaptors.
- `IEntryRepository`: For accessing entry data.
- `ISchemaMerger`: For merging schemas.

### Constructor

The constructor initializes the `TraefikController` with the required dependencies.

```csharp
public TraefikController(IHostEnvironment hostEnvironment,
    ISettingsToSchemaFacade settingsToSchemaFacade,
    IOptionsSnapshot<MainSettings> optionsSnapshot,
    ILogger<TraefikController> logger,
    ISchemaMerger schemaMerger)
{
    _settingsToSchemaFacade = settingsToSchemaFacade;
    _optionsSnapshot = optionsSnapshot;
    _logger = logger;
    _schemaMerger = schemaMerger;
}
```

#### Don't forget to pull docker image instead 
```docker pull turrican/iwmm:latest```

