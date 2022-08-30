using Eccc.Sali;
using Newtonsoft.Json;
using System;

namespace TABS.Audit
{
    // Not in use currently, left here in case we have to revert AuditLogService approach.
    public class TABSAuditLog
    {
        public const string ADAUTHENTICATOR = "ActiveDirectory";

        public static void LogCrudAction(string crudAction, object obj, AuditLog.ActionCategory action, AuditLog.SeverityCategory severity, AuditLog.ResultCategory result, Exception e = null)
        {
            Sali.Log(action, severity, obj.GetType().ToString(), JsonConvert.SerializeObject(obj),
                ADAUTHENTICATOR, "userId", "ipAddress", "CrudService:" + crudAction, result, e);
        }
    }
}
