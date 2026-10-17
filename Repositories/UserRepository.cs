using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace CafePOS;

public class UserRepository : IRepository<User>
{
    const string Cols = "UserId,Username,FullName,Role,IsActive";

    public static string Hash(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }

    static User Map(DataRow r) => new()
    {
        Id = (int)r["UserId"],
        Username = (string)r["Username"],
        FullName = (string)r["FullName"],
        Role = (string)r["Role"],
        IsActive = (bool)r["IsActive"]
    };

    public List<User> GetAll() =>
        Db.Query($"SELECT {Cols} FROM Users ORDER BY Username").Rows.Cast<DataRow>().Select(Map).ToList();

    public User? Authenticate(string username, string password)
    {
        var dt = Db.Query($"SELECT {Cols} FROM Users WHERE Username=@u AND PasswordHash=@h AND IsActive=1",
            ("@u", username), ("@h", Hash(password)));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    public void Create(string username, string password, string fullName, string role) =>
        Db.Exec("INSERT INTO Users(Username,PasswordHash,FullName,Role) VALUES(@u,@h,@f,@r)",
            ("@u", username), ("@h", Hash(password)), ("@f", fullName), ("@r", role));

    /// <summary>Creates default accounts the first time the app runs.</summary>
    public void EnsureSeed()
    {
        var n = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Users"));
        if (n > 0) return;
        Create("admin", "admin123", "System Admin", "Admin");
        Create("manager", "manager123", "Store Manager", "Manager");
        Create("cashier", "cashier123", "Cashier 01", "Cashier");
    }
}
