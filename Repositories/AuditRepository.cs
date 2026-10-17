using System.Data;

namespace CafePOS;

public class AuditRepository
{
    public void Log(string action, string details)
    {
        try
        {
            Db.Exec("INSERT INTO AuditLog(UserId,Username,Action,Details) VALUES(@i,@u,@a,@d)",
                ("@i", Session.CurrentUser?.Id), ("@u", Session.CurrentUser?.Username),
                ("@a", action), ("@d", details));
        }
        catch { /* never block the cashier because logging failed */ }
    }

    public DataTable Search(DateTime from, DateTime to, string keyword) =>
        Db.Query(@"SELECT LogDate AS [Date], Username AS [User], Action, Details
                   FROM AuditLog
                   WHERE LogDate >= @f AND LogDate < @t
                     AND (@k = '' OR Username LIKE '%'+@k+'%' OR Action LIKE '%'+@k+'%' OR Details LIKE '%'+@k+'%')
                   ORDER BY LogDate DESC",
            ("@f", from.Date), ("@t", to.Date.AddDays(1)), ("@k", keyword));
}
