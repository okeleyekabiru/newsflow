using Hangfire.Dashboard;

namespace NewsFlow.API;

/// <summary>
/// Restricts the Hangfire dashboard to authenticated users who hold the
/// <c>Admin</c> role.  In development mode every authenticated user is
/// permitted so the dashboard can be inspected without assigning roles.
/// </summary>
public sealed class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireAuthFilter(IWebHostEnvironment env) => _env = env;

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        // Must be logged in
        if (http.User.Identity?.IsAuthenticated != true)
            return false;

        // In development any authenticated user can view the dashboard
        if (_env.IsDevelopment())
            return true;

        // In all other environments require the Admin role
        return http.User.IsInRole("Admin");
    }
}
