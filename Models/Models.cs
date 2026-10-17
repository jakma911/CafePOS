namespace CafePOS;

// ---------- OOP: base class, inheritance, encapsulation, interface ----------

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public interface IRepository<T>
{
    List<T> GetAll();
}

public class User : BaseEntity
{
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Cashier";
    public bool IsActive { get; set; } = true;
    public bool CanApprove => Role == "Admin" || Role == "Manager";
}

public class Product : BaseEntity
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public override string ToString() => Name;
}

public class CafeTable : BaseEntity
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Available";
    public bool IsAvailable => Status == "Available";
    public override string ToString() => Name;
}

public class OrderLine
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total => Qty * UnitPrice;
}

public class Order : BaseEntity
{
    public string OrderNo { get; set; } = "";
    public int TableId { get; set; }
    public string TableName { get; set; } = "";
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Open";
    public List<OrderLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Total);
}

public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public decimal Received { get; set; }
    public decimal Change { get; set; }
    public DateTime PaidDate { get; set; } = DateTime.Now;
}

/// <summary>Holds the currently logged in user.</summary>
public static class Session
{
    public static User? CurrentUser { get; set; }
}
