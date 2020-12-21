# MultiTenancyServer.Samples

See [MultiTenancyServer README](https://github.com/MultiTenancyServer/MultiTenancyServer).

All examples will register two tenants (tenant1 and tenant2) along with three users with one user, alice, registered within both tenants. You should launch a sample project as an exe when debugging instead of via IIS Express so the /seed command line argument is passed in which will generate the Sqlite database(s) in the project folder and populate with the above mentioned tenants and users. All user passwords are Pass123$.

|User|Tenant|Password|
|---|---|---|
|alice@contoso.com|tenant1 & tenant2|Pass123$|
|bob@contoso.com|tenant1|Pass123$|
|chris@contoso.com|tenant2|Pass123$|

## ASP.NET Core Identity
Sample project: [AspNetIdentityAndEFCore](https://github.com/MultiTenancyServer/MultiTenancyServer/tree/master/samples/AspNetIdentityAndEFCore)<br />
Components: ASP.NET Core Identity and Entity Framework Core<br/>
Database model: single database with tenancy shadow columns<br/>

## ASP.NET Core Identity using Int64
Sample project: [Int64AspNetIdentityAndEFCore]https://github.com/MultiTenancyServer/MultiTenancyServer/tree/master/samples/Int64AspNetIdentityAndEFCore)<br />
Components: ASP.NET Core Identity and Entity Framework Core<br/>
Database model: single database with tenancy shadow columns<br/>
