using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancyServer.Samples.AspNetIdentityAndEFCore.Data;
using MultiTenancyServer.Samples.AspNetIdentityAndEFCore.Models;

namespace MultiTenancyServer.Samples.AspNetIdentityAndEFCore
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Seeding database...");

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<ApplicationTenant>>();
                var tenant1 = tenantMgr.FindByCanonicalNameAsync("tenant1").Result;
                if (tenant1 == null)
                {
                    tenant1 = new ApplicationTenant
                    {
                        CanonicalName = "tenant1",
                        DisplayName = "Tenant One",
                    };
                    var result = tenantMgr.CreateAsync(tenant1).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant1 created");
                }
                else
                {
                    Console.WriteLine("tenant1 already exists");
                }

                var tenant2 = tenantMgr.FindByCanonicalNameAsync("tenant2").Result;
                if (tenant2 == null)
                {
                    tenant2 = new ApplicationTenant
                    {
                        CanonicalName = "tenant2",
                        DisplayName = "Tenant Two",
                    };
                    var result = tenantMgr.CreateAsync(tenant2).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant2 created");
                }
                else
                {
                    Console.WriteLine("tenant2 already exists");
                }
            }

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<ApplicationTenant>>();
                var tenant = tenantMgr.FindByCanonicalNameAsync("Tenant1").Result;
                var tenancyContext = scope.ServiceProvider.GetService<ITenancyContext<ApplicationTenant>>();
                tenancyContext.Tenant = tenant;

                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alice = userMgr.FindByNameAsync("alice@contoso.com").Result;
                if (alice == null)
                {
                    alice = new ApplicationUser
                    {
                        UserName = "alice@contoso.com",
                        Email = "alice@contoso.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("alice created");
                }
                else
                {
                    Console.WriteLine("alice already exists");
                }

                var bob = userMgr.FindByNameAsync("bob@contoso.com").Result;
                if (bob == null)
                {
                    bob = new ApplicationUser
                    {
                        UserName = "bob@contoso.com",
                        Email = "bob@contoso.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("bob created");
                }
                else
                {
                    Console.WriteLine("bob already exists");
                }
            }

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<ApplicationTenant>>();
                var tenant = tenantMgr.FindByCanonicalNameAsync("Tenant2").Result;
                var tenancyContext = scope.ServiceProvider.GetService<ITenancyContext<ApplicationTenant>>();
                tenancyContext.Tenant = tenant;

                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alice = userMgr.FindByNameAsync("alice@contoso.com").Result;
                if (alice == null)
                {
                    alice = new ApplicationUser
                    {
                        UserName = "alice@contoso.com",
                        Email = "alice@contoso.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("alice created");
                }
                else
                {
                    Console.WriteLine("alice already exists");
                }

                var chris = userMgr.FindByNameAsync("chris@contoso.com").Result;
                if (chris == null)
                {
                    chris = new ApplicationUser
                    {
                        UserName = "chris@contoso.com",
                        Email = "chris@contoso.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(chris, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("chris created");
                }
                else
                {
                    Console.WriteLine("chris already exists");
                }
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }
    }
}
