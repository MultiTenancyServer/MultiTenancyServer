[![Build status](https://ci.appveyor.com/api/projects/status/6by9bawg017k26tl/branch/master?svg=true)](https://ci.appveyor.com/project/krispenner/multitenancyserver/branch/master)
# MultiTenancyServer

MultiTenancyServer aims to be a lightweight package for adding multi-tenancy support to any codebase easily. It is heavily influenced from the design of ASP.NET Core Identity. You can add multi-tenancy support to your model without adding any tenant partition key properties to any classes/entities. Using ASP.NET Core it can retrieve the current tenant from a custom domain name, partial hostname, HTTP request header, URL path, query string parameter, authenticated user claim, or a custom implementation. Using Entity Framework Core, tenant partition keys are added as shadow properties (or optionally concrete properties) and enforced through global query filters. The below example shows how to use MultiTenancyServer with ASP.NET Core Identity and IdentityServer4 together, if you only need one remove the other or use this as a base for your own requirements.

## Define Model
Define your own tenant model, or inherit from TenancyTenant, or just use TenancyTenant as is. In this example we will inherit from TenancyTenant.

``` cs
public class Tenant : TenancyTenant
{
    public string Name { get; set; }
}
```

## Register Services
Example of adding multi-tenancy support to ASP.NET Core.
``` cs
public void ConfigureServices(IServiceCollection services)
{
    services
        // Add Multi-Tenancy Server defining TTenant<TKey>
        // as type Tenant with an ID (key) of type string.
        .AddMultiTenancy<Tenant, string>()
        // Requires MultiTenancyServer.AspNetCore
        // Add the HTTP request parser based on your requirements,
        // or implement your own IRequestParser.
        .AddRequestParsers(parsers =>
        {
            // Parsers are processed in the order they are added.
            // Typically 1 or 2 parsers should be all you need.
            parsers
                // www.tenant1.com
                .AddDomainParser()
                // tenant1.tenants.multitenancyserver.io
                .AddHostParserForParent(".tenants.multitenancyserver.io")
                // HTTP header X-TENANT = tenant1
                .AddHeaderParser("X-TENANT")
                // /tenants/tenant1
                .AddPathParserForParent("/tenants/")
                // ?tenant=tenant1
                .AddQueryParser("tenant")
                // Get user claim http://schemas.microsoft.com/identity/claims/tenantid
                // from authenticated user principal.
                .AddClaimParser("http://schemas.microsoft.com/identity/claims/tenantid");
        })
        // Use in memory tenant store for testing (MultiTenancyServer.Stores)
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
        // Or implement your own store...
        .AddMongoDbStore();
}    
```

## Register Entities
Example of DbContext with multi-tenancy support for ASP.NET Core Identity and IdentityServer4.
``` cs
    public class AppDbContext : 
        // ASP.NET Core Identity EF Core
        IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, 
        // IdentityServer4 EF Core
        IConfigurationDbContext, IPersistedGrantDbContext,
        // MultiTenancyServer EF Core
        ITenantDbContext<Tenant, string>
    {
        private static object _tenancyModelState;
        private readonly string _tenantId;
        private readonly ILogger _logger;

        public SecuridDbContext(
            DbContextOptions<SecuridDbContext> options, 
            ITenancyProvider<Tenant> tenancyProvider, 
            ILogger<AppDbContext> logger)
            : base(options)
        {
            // The ID of the currently scoped tenant (of the current HTTP request).
            // While this method is async to facilitate async scenarios,
            // the built-in provider for ASP.NET Core will return immediately.
            _tenantId = tenancyProvider?.GetCurrentTenantAsync().GetAwaiter().GetResult()?.Id;
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
            // Ensure multi-tenancy for all tenatable entities.
            this.EnsureTenancy(_tenantId, _tenancyModelState, _logger);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Ensure multi-tenancy for all tenatable entities.
            this.EnsureTenancy(_tenantId, _tenancyModelState, _logger);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
```
