using Hangfire.Dashboard;
using System.Net;

namespace Ibero.CnbAutomatizacion.API.Filters;

public class HangfireDashboardAuthorizationFilter(IWebHostEnvironment env) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // En desarrollo, acceso libre para facilitar el trabajo local
        if (env.IsDevelopment()) return true;

        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp == null) return false;

        // En producción: solo desde localhost (acceso vía RDP al servidor IIS)
        return IPAddress.IsLoopback(remoteIp) ||
               Equals(remoteIp, httpContext.Connection.LocalIpAddress);
    }
}
