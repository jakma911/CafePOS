using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOS;

/// <summary>
/// Simple ADO.NET helper.
/// The server is detected automatically (LocalDB, SQLEXPRESS, localhost).
/// To force a specific server, set the CAFEPOS_CONN environment variable,
/// or put your own connection string in ManualConnectionString below.
/// </summary>
public static class Db
{
    // Leave empty for auto-detect. Example:
    // "Server=localhost;Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;"
    public static string ManualConnectionString = "";

    static readonly string[] Candidates =
    {
        "(localdb)\\ProjectModels",
        "(localdb)\\MSSQLLocalDB",
        ".\\SQLEXPRESS",
        "localhost",
        "."
    };

    static string? resolved;

    static string Build(string server) =>
        $"Server={server};Database=CafePOS;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";

    static string Resolve()
    {
        if (resolved != null) return resolved;

        var env = Environment.GetEnvironmentVariable("CAFEPOS_CONN");
        if (!string.IsNullOrWhiteSpace(env)) return resolved = env;
        if (!string.IsNullOrWhiteSpace(ManualConnectionString)) return resolved = ManualConnectionString;

        var errors = new List<string>();
        foreach (var server in Candidates)
        {
            var cs = Build(server);
            try
            {
                using var c = new SqlConnection(cs);
                c.Open();
                return resolved = cs;
            }
            catch (Exception ex)
            {
                errors.Add($"{server}: {ex.Message}");
            }
        }
        throw new InvalidOperationException(
            "Could not connect to any SQL Server. Tried:\n" + string.Join("\n", errors) +
            "\n\nInstall SQL Server Express/LocalDB, run Database/CafePOS.sql, " +
            "or set Db.ManualConnectionString.");
    }

    public static SqlConnection Open()
    {
        var c = new SqlConnection(Resolve());
        c.Open();
        return c;
    }

    static SqlCommand Make(SqlConnection c, SqlTransaction? t, string sql, (string Name, object? Value)[] ps)
    {
        var cmd = c.CreateCommand();
        cmd.Transaction = t;
        cmd.CommandText = sql;
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return cmd;
    }

    public static DataTable Query(string sql, params (string Name, object? Value)[] ps)
    {
        using var c = Open();
        using var cmd = Make(c, null, sql, ps);
        using var ad = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        ad.Fill(dt);
        return dt;
    }

    public static int Exec(string sql, params (string Name, object? Value)[] ps)
    {
        using var c = Open();
        using var cmd = Make(c, null, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, params (string Name, object? Value)[] ps)
    {
        using var c = Open();
        using var cmd = Make(c, null, sql, ps);
        var r = cmd.ExecuteScalar();
        return r == DBNull.Value ? null : r;
    }

    public static int Exec(SqlConnection c, SqlTransaction t, string sql, params (string Name, object? Value)[] ps)
    {
        using var cmd = Make(c, t, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static object? Scalar(SqlConnection c, SqlTransaction t, string sql, params (string Name, object? Value)[] ps)
    {
        using var cmd = Make(c, t, sql, ps);
        var r = cmd.ExecuteScalar();
        return r == DBNull.Value ? null : r;
    }
}
