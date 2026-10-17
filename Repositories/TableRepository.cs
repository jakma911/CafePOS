using System.Data;

namespace CafePOS;

public class TableRepository : IRepository<CafeTable>
{
    public List<CafeTable> GetAll() =>
        Db.Query("SELECT TableId,TableName,Status FROM CafeTables ORDER BY TableName")
          .Rows.Cast<DataRow>()
          .Select(r => new CafeTable { Id = (int)r["TableId"], Name = (string)r["TableName"], Status = (string)r["Status"] })
          .ToList();
}
