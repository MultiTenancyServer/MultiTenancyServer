using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.EntityFramework;
using MultiTenancyServer.Options;
using MultiTenancyServer.Samples.AspNetIdentityAndEFCore.Models;

namespace MultiTenancyServer.Samples.AspNetIdentityAndEFCore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, ITenantDbContext<ApplicationTenant, string>
    {
        private static TenancyModelState<string> _tenancyModelState;
        private readonly ITenancyContext<ApplicationTenant> _tenancyContext;
        private readonly ILogger _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenancyContext<ApplicationTenant> tenancyContext,
            ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            // The request scoped tenancy context.
            // Should not access the tenancyContext.Tenant property in the constructor yet,
            // as the request pipeline has not finished running yet and it will likely be null.
            // Use the private property wrapper above to access it later on demand.
            _tenancyContext = tenancyContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // MultiTenancyServer implementation.
        public DbSet<ApplicationTenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // MultiTenancyServer configuration.
            var tenantStoreOptions = new TenantStoreOptions();
            builder.ConfigureTenantContext<ApplicationTenant, string>(tenantStoreOptions);

            // Add multi-tenancy support to model.
            var tenantReferenceOptions = new TenantReferenceOptions();
            builder.HasTenancy<string>(tenantReferenceOptions, out _tenancyModelState);

            // Configure custom properties on ApplicationTenant.
            builder.Entity<ApplicationTenant>(b =>
            {
                b.Property(t => t.DisplayName).HasMaxLength(256);
            });

            // Configure properties on User (ASP.NET Core Identity).
            builder.Entity<ApplicationUser>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenancyContext.Tenant.Id, _tenancyModelState, hasIndex: false);
                // Remove unique index on NormalizedUserName.
                b.HasIndex(u => u.NormalizedUserName).HasName("UserNameIndex").IsUnique(false);
                // Add unique index on TenantId and NormalizedUserName.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(ApplicationUser.NormalizedUserName))
                    .HasName("TenantUserNameIndex").IsUnique();
            });

            // Configure properties on Role (ASP.NET Core Identity).
            builder.Entity<IdentityRole>(b =>
            {
                // Add multi-tenancy support to entity.
                b.HasTenancy(() => _tenancyContext.Tenant.Id, _tenancyModelState, hasIndex: false);
                // Remove unique index on NormalizedUserName.
                b.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique(false);
                // Add unique index on TenantId and NormalizedUserName.
                b.HasIndex(tenantReferenceOptions.ReferenceName, nameof(IdentityRole.NormalizedName))
                    .HasName("TenantRoleNameIndex").IsUnique();
            });
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Ensure multi-tenancy for all tenantable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
