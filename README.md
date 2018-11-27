[![Build status](https://ci.appveyor.com/api/projects/status/6by9bawg017k26tl/branch/master?svg=true)](https://ci.appveyor.com/project/krispenner/multitenancyserver/branch/master)
# MultiTenancyServer

MultiTenancyServer aims to be a lightweight package for adding multi-tenancy support to any codebase easily. It is heavily influenced from the design of ASP.NET Core Identity. You can add multi-tenancy support to your model without adding any tenant key properties to any classes or entities. Using ASP.NET Core, the current tenant can be retrieved by a custom domain name, sub-domain, partial hostname, HTTP request header, child or partial URL path, query string parameter, authenticated user claim, or a custom request parser implementation. Using Entity Framework Core, tenant keys are added as shadow properties (or optionally concrete properties) and enforced through global query filters, all configurable options can be set from a default or  per entity. The below example shows how to use MultiTenancyServer with ASP.NET Core Identity and IdentityServer4 together, if you only need one remove the other or use this as a base for your own requirements. You can find a full working sample integrated with ASP.NET Core Identity and Entity Framework Core [here](https://github.com/MultiTenancyServer/MultiTenancyServer.Samples).

## Define Model
Define your own tenant model, or inherit from TenancyTenant, or just use TenancyTenant as is. In this example we will inherit from TenancyTenant.

``` csharp
public class Tenant : TenancyTenant
{
    // Custom property for display name of tenant.
    public string Name { get; set; }
}
```

## Register Services
Example of adding multi-tenancy support to ASP.NET Core.
``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        // Add Multi-Tenancy Server defining TTenant<TKey> as type Tenant with an ID (key) of type string.
        .AddMultiTenancy<Tenant, string>()
        // Add one or more IRequestParser (MultiTenancyServer.AspNetCore).
        .AddRequestParsers(parsers =>
        {
            // Parsers are processed in the order they are added,
            // typically 1 or 2 parsers should be all you need.
            parsers
                // www.tenant1.com
                .AddDomainParser()
                // tenant1.tenants.multitenancyserver.io
                .AddSubdomainParser(".tenants.multitenancyserver.io")
                // from partial hostname
                .AddHostnameParser("^(regular_expression)$")
                // HTTP header X-TENANT = tenant1
                .AddHeaderParser("X-TENANT")
                // /tenants/tenant1
                .AddChildPathParser("/tenants/")
                // from partial path
                .AddPathParser("^(regular_expression)$")
                // ?tenant=tenant1
                .AddQueryParser("tenant")
                // Claim from authenticated user principal.
                .AddClaimParser("http://schemas.microsoft.com/identity/claims/tenantid")
                // Add custom request parser with lambda.
                .AddCustomParser(httpContext => "tenant1");
                // Add custom request parser implementation.
                .AddMyCustomParser();
        })
        // Use in memory tenant store for development (MultiTenancyServer.Stores)
        .AddInMemoryStore(new Tenant[] 
        { 
            new Tenant() 
            { 
                Id = "TENANT_1", 
                CanonicalName = "Tenant1", 
                NormalizedCanonicalName = "TENANT1"
            }
        })
        // Use EF Core store for production (MultiTenancyServer.EntityFrameworkCore).
        .AddEntityFrameworkStore<AppDbContext, Tenant, string>()
        // Use custom store.
        .AddMyCustomStore();
}    
```

## Add Middleware
Example of configuring application with multi-tenancy support for ASP.NET Core MVC and IdentityServer4.
``` csharp
public void Configure(IApplicationBuilder app)
{
    // other code removed for brevity

    app.UseMultiTenancy<Tenant>();
    app.UseIdentityServer();
    app.UseMvcWithDefaultRoute();
}
```

## Configure Entities
Example of DbContext with multi-tenancy support for ASP.NET Core Identity and IdentityServer4.
``` csharp
    public class AppDbContext : 
        // ASP.NET Core Identity EF Core
        IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, 
        // IdentityServer4 EF Core
        IConfigurationDbContext, IPersistedGrantDbContext,
        // MultiTenancyServer EF Core
        ITenantDbContext<Tenant, string>
    {
        private static object _tenancyModelState;
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly ILogger _logger;
        // Use a property wrapper to access the scoped tenant on demand.
        private object _tenantId => _tenancyContext?.Tenant?.Id;

        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            ITenancyContext<Tenant> tenancyContext, 
            ILogger<AppDbContext> logger)
            : base(options)
        {
            // The request scoped tenancy context.
            // Should not access the tenancyContext.Tenant property in the constructor yet,
            // as the request pipeline has not finished running yet and it will likely be null.
            // Use the private property wrapper above to access it later on demand.
            _tenancyContext = tenancyContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // IdentityServer4 implementation.
        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }

        // MultiTenancyServer implementation.
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // IdentityServer4 configuration.
            var configurationStoreOptions = new ConfigurationStoreOptions();
            builder.ConfigureClientContext(configurationStoreOptions);
            builder.ConfigureResourcesContext(configurationStoreOptions);
            var operationalStoreOptions = new OperationalStoreOptions();
            builder.ConfigurePersistedGrantContext(operationalStoreOptions);

            // MultiTenancyServer configuration.
            var tenantStoreOptions = new TenantStoreOptions();
            builder.ConfigureTenantContext<Tenant, string>(tenantStoreOptions);

            // Add multi-tenancy support to model.
            var tenantReferenceOptions = new TenantReferenceOptions();
            builder.HasTenancy<string>(tenantReferenceOptions, out _tenancyModelState);

            // Configure custom properties on Tenant (MultiTenancyServer).
            builder.Entity<Tenant>(b =>
            {
                b.Property(t => t.Name).HasMaxLength(256);
            });

            // Configure properties on User (ASP.NET Core Identity).
            builder.Entity<User>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on NormalizedUserName.
                b.HasIndex(u => u.NormalizedUserName).HasName("UserNameIndex").IsUnique(false);
                // Add unique index on TenantId and NormalizedUserName.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(User.NormalizedUserName))
                    .HasName("TenantUserNameIndex").IsUnique();
            });

            // Configure properties on Role (ASP.NET Core Identity).
            builder.Entity<Role>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on NormalizedUserName.
                b.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique(false);
                // Add unique index on TenantId and NormalizedUserName.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(Role.NormalizedName))
                    .HasName("TenantRoleNameIndex").IsUnique();
            });

            // Configure properties on Client (IdentityServer4).
            builder.Entity<Client>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on ClientId.
                b.HasIndex(c => c.ClientId).IsUnique(false);
                // Add unique index on TenantId and ClientId.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(Client.ClientId)).IsUnique();
            });

            // Configure properties on IdentityResource (IdentityServer4).
            builder.Entity<IdentityResource>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on Name.
                b.HasIndex(r => r.Name).IsUnique(false);
                // Add unique index on TenantId and Name.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(IdentityResource.Name)).IsUnique();
            });

            // Configure properties on ApiResource (IdentityServer4).
            builder.Entity<ApiResource>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on Name.
                b.HasIndex(r => r.Name).IsUnique(false);
                // Add unique index on TenantId and Name.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(ApiResource.Name)).IsUnique();
            });

            // Configure properties on ApiScope (IdentityServer4).
            builder.Entity<ApiScope>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState, hasIndex: false);
                // Remove unique index on Name.
                b.HasIndex(s => s.Name).IsUnique(false);
                // Add unique index on TenantId and Name.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(ApiScope.Name)).IsUnique();
            });

            // Configure properties on PersistedGrant (IdentityServer4).
            builder.Entity<PersistedGrant>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenantId, _tenancyModelState);
            });
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenantId, _tenancyModelState, _logger);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenantId, _tenancyModelState, _logger);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
```

## Additional Options

``` csharp
public class TenantReferenceOptions
{
    // Summary:
    //     If set to a non-null value, the store will use this value as the name for the
    //     tenant's reference property. The default is "TenantId".
    public string ReferenceName { get; set; }

    // Summary:
    //     True to enable indexing of tenant reference properties in the store, otherwise
    //     false. The default is true.
    public bool IndexReferences { get; set; }

    // Summary:
    //     If set to a non-null value, the store will use this value as the name of the
    //     index for any tenant references. The name is also a format pattern of {0:PropertyName}.
    //     The default is "{0}Index", eg. "TenantIdIndex".
    public string IndexNameFormat { get; set; }

    // Summary:
    //     Determines if a null tenant reference is allowed for entities and how querying
    //     for null tenant references is handled.
    public NullTenantReferenceHandling NullTenantReferenceHandling { get; set; }
}

public enum NullTenantReferenceHandling
{
    // Summary:
    //     A null tenant reference is NOT allowed for the entity, where possible a NOT NULL
    //     or REQUIRED constraint should be set on the tenant reference, querying for entities
    //     with a null tenant reference will match NO entities.
    //     This is the default option.
    NotNullDenyAccess = 0,

    // Summary:
    //     A null tenant reference is allowed for the entity, where possible an NULLABLE
    //     or OPTIONAL constraint should be set on the tenant reference, querying for entities
    //     with a null tenant reference will match those expected results.
    //     This may be useful where globally defined system entities are set with a null
    //     tenant reference.
    NullableEntityAccess = 1,

    // Summary:
    //     A null tenant reference is NOT allowed for the entity, where possible a NOT NULL
    //     or REQUIRED constraint should be set on the tenant reference, querying for entities
    //     with a null tenant reference will match ALL entities across all tenants.
    //     For obvious security reasons, this is typically not recommended; however, this
    //     can be useful for admin reporting across all tenants.
    NotNullGlobalAccess = 2
}

```

## Interesting Logs

### Microsoft.AspNetCore start of request

> Microsoft.AspNetCore.Hosting.Internal.WebHost:Information: Request starting HTTP/1.1 POST http://**localhost**:5020/account/login?returnUrl=%2Fgrants application/x-www-form-urlencoded 267

### MultiTenancyServer.EntityFrameworkCore lookup tenant

> Microsoft.EntityFrameworkCore.Database.Command:Information: Executed DbCommand (3ms) [Parameters=[@__normalizedCanonicalName_0='**LOCALHOST**' (Size = 256)], CommandType='Text', CommandTimeout='30']
SELECT TOP(1) [u].[Id], [u].[CanonicalName], [u].[ConcurrencyStamp], [u].[Name], [u].[NormalizedCanonicalName]
FROM [Tenants] AS [u]
WHERE [u].[NormalizedCanonicalName] = @__normalizedCanonicalName_0

### MultiTenancyServer.AspNetCore found tenant

>MultiTenancyServer.Http.HttpTenancyProvider:Debug: Tenant **TEST_TENANT_1** found by **DomainParser** for value **localhost** in request http://**localhost**:5020/account/login?returnUrl=%2Fgrants.

### Microsoft.AspNetCore.Identity lookup user within tenant

> Microsoft.EntityFrameworkCore.Database.Command:Information: Executed DbCommand (13ms) [Parameters=[@___tenantId_0='**TEST_TENANT_1**' (Size = 4000), @__normalizedUserName_0='ALICE' (Size = 256)], CommandType='Text', CommandTimeout='30']
SELECT TOP(1) [u].[Id], ..., [u].[UserName]
FROM [Users] AS [u]
WHERE **(@___tenantId_0 IS NOT NULL AND ([u].[TenantId] = @___tenantId_0))** AND ([u].[NormalizedUserName] = @__normalizedUserName_0)

### IdentityServer4 lookup persisted grant within tenant

> Microsoft.EntityFrameworkCore.Database.Command:Information: Executed DbCommand (1ms) [Parameters=[@___tenantId_0='**TEST_TENANT_1**' (Size = 4000), @__subjectId_0='3ab99036-8ac1-4270-8a1e-390988966b9c' (Size = 200)], CommandType='Text', CommandTimeout='30']
SELECT [p].[Key], [p].[ClientId], [p].[CreationTime], [p].[Data], [p].[Expiration], [p].[SubjectId], [p].[TenantId], [p].[Type]
FROM [PersistedGrants] AS [p]
WHERE **(@___tenantId_0 IS NOT NULL AND ([p].[TenantId] = @___tenantId_0))** AND ([p].[SubjectId] = @__subjectId_0)
