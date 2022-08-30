using Eccc.Sali;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TABS.Data;

namespace TABS.Audit
{
    public class AuditLogService
    {
        private const string ADAUTHENTICATOR = "ActiveDirectory";

        private readonly AuthService _authService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly NavigationManager _navigationManager;

        public AuditLogService(AuthService authService, IHttpContextAccessor httpContextAccessor, NavigationManager navigationManager)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _navigationManager = navigationManager;
        }

        private string GetClientIPAddress()
        {
            string ip = string.Empty;
            HttpContext context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return ip;
            }
            if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
            {
                ip = context.Request.Headers["X-Forwarded-For"];
            }
            else
            {
                ip = context.Request.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString();
            }
            return ip;
        }

        public async Task LogAsync(object obj, AuditLog.ActionCategory action, AuditLog.SeverityCategory severity, AuditLog.ResultCategory result, Exception e = null)
        {
            string userId = "<UNKNOWN SID>";
            string clientIP = "<UNKNOWN IP ADDRESS>";
            string url = "<UNKNOWN URL>";

            try
            {
                userId = await _authService.GetUserSID();
                clientIP = GetClientIPAddress();
                url = _navigationManager.Uri;
            }
            catch
            {
                // don't do anything - values are already defaulted
            }

            Sali.Log(action, severity, obj.GetType().ToString(), JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                ADAUTHENTICATOR, userId, clientIP, url, result, e);
        }
    }
}
