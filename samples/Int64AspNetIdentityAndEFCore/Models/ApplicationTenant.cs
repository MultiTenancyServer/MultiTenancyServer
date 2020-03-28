namespace MultiTenancyServer.Samples.AspNetIdentityAndEFCore.Models
{
    // Add profile data for application tenants by adding properties to the ApplicationTenant class
    public class ApplicationTenant : TenancyTenant<long>
    {
        public string DisplayName { get; set; }
    }
}
