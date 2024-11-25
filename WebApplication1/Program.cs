using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Security.Principal;
using WebApplication1;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register repositories with dependency injection (as already done in your code)
builder.Services.AddTransient<BuildRepository>(sp =>
    new BuildRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<BuildRepository.DMCRRequestContext>(sp =>
    new BuildRepository.DMCRRequestContext(builder.Configuration.GetConnectionString("DMCRDBConnectionString")));

builder.Services.AddTransient<BuildRepository.ScmDbContext>(sp =>
    new BuildRepository.ScmDbContext(builder.Configuration.GetConnectionString("ScmDefaultConnection")));

builder.Services.AddScoped<ScriptRepository>();
builder.Services.AddTransient<ServerService>();

// Register IDbConnection for SCM Database
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("ScmDefaultConnection");
    return new SqlConnection(connectionString);
});

// Register IDbConnection for TD Database
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// Register SCMBuildHelper with custom connections
builder.Services.AddScoped<ISCMBuildHelper>(sp =>
{
    var scmConnection = sp.GetRequiredService<IDbConnection>();
    var tdConnection = sp.GetRequiredService<IDbConnection>();
    return new SCMBuildHelper(scmConnection, tdConnection);
});

// Configure Negotiate (Windows) Authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Configure Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("WindowsPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add(NegotiateDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            return user.Identity is WindowsIdentity windowsIdentity &&
                   (windowsIdentity.Name == @"THESYSTEM\nikhilsi!" ||
                    windowsIdentity.Name == @"THESYSTEM\nikhilsi" ||
                    windowsIdentity.Name == @"DOMAIN\UserName3");
        });
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

