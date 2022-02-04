# IWMM (IP Whitelist Middleware Manager)
## Useful tool to manage FQDN whitelisting rules for multiple webservers

Just use settings like this :

```
MainSettings:
  IpWhiteListSettings:
    - SchemaType: TraefikIpWhitelistMiddlewareFile
      TraefikMiddlewareSettings:
        Name: test1TestAllowed
        FilePath: "pisosTest1Allowed.yml"
    - SchemaType: TraefikIpWhitelistMiddlewareFile
      TraefikMiddlewareSettings:
        Name: test2TestAllowed
        FilePath: "pisosTest2Allowed.yml"        
      AllowedEntries:
        - Turrican
        - Office
  Entries:
    - Name: Turrican
      Fqdn: turrican.soundcast.me
    - Name: Office
      Fqdn: www.habitatsoft.com
```
