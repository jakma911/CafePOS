using System.Data;

namespace CafePOS;

public class ProductRepository : IRepository<Product>
{
    static Product Map(DataRow r) => new()
    {
        Id = (int)r["ProductId"],
        Name = (string)r["Name"],
        Category = (string)r["Category"],
        Price = (decimal)r["Price"],
        IsActive = (bool)r["IsActive"]
    };

    public List<Product> GetAll() => GetAll(false);

    public List<Product> GetAll(bool activeOnly) =>
        Db.Query("SELECT ProductId,Name,Category,Price,IsActive FROM Products " +
                 (activeOnly ? "WHERE IsActive=1 " : "") + "ORDER BY Category,Name")
          .Rows.Cast<DataRow>().Select(Map).ToList();

    public void Add(Product p) =>
        Db.Exec("INSERT INTO Products(Name,Category,Price) VALUES(@n,@c,@p)",
            ("@n", p.Name), ("@c", p.Category), ("@p", p.Price));

    public void Update(Product p) =>
        Db.Exec("UPDATE Products SET Name=@n,Category=@c,Price=@p,IsActive=@a WHERE ProductId=@id",
            ("@n", p.Name), ("@c", p.Category), ("@p", p.Price), ("@a", p.IsActive), ("@id", p.Id));

    /// <summary>Soft delete so old orders keep their product reference.</summary>
    public void Deactivate(int id) =>
        Db.Exec("UPDATE Products SET IsActive=0 WHERE ProductId=@id", ("@id", id));
}
