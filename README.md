# IWMM (IP Whitelist Middleware Manager)
## Useful tool to manage FQDN whitelisting rules for multiple webservers

Just use settings like this :

```
MainSettings:
  IpWhiteListSettings:
    - SchemaType: TraefikIpWhitelistMiddlewareFile
      TraefikMiddlewareSettings:
        Name: test1TestAllowed
        FilePath: "test1Allowed.yml"
    - SchemaType: TraefikIpWhitelistMiddlewareFile
      TraefikMiddlewareSettings:
        Name: test2TestAllowed
        FilePath: "test2Allowed.yml"        
      AllowedEntries:
        - Test1
        - Office
  Entries:
    - Name: Test1
      Fqdn: test.com
    - Name: Office
      Fqdn: www.office.com
```
