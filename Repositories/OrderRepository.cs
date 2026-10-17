using System.Data;

namespace CafePOS;

public class OrderRepository
{
    public Order? GetOpenOrder(int tableId)
    {
        var dt = Db.Query(@"SELECT o.OrderId,o.OrderNo,o.TableId,t.TableName,o.UserId,o.OrderDate,o.Status
                            FROM Orders o JOIN CafeTables t ON t.TableId=o.TableId
                            WHERE o.TableId=@t AND o.Status='Open'", ("@t", tableId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        var o = new Order
        {
            Id = (int)r["OrderId"],
            OrderNo = (string)r["OrderNo"],
            TableId = (int)r["TableId"],
            TableName = (string)r["TableName"],
            UserId = (int)r["UserId"],
            OrderDate = (DateTime)r["OrderDate"],
            Status = (string)r["Status"]
        };
        var items = Db.Query(@"SELECT i.ProductId,p.Name,i.Qty,i.UnitPrice
                               FROM OrderItems i JOIN Products p ON p.ProductId=i.ProductId
                               WHERE i.OrderId=@o", ("@o", o.Id));
        foreach (DataRow x in items.Rows)
            o.Lines.Add(new OrderLine
            {
                ProductId = (int)x["ProductId"],
                ProductName = (string)x["Name"],
                Qty = (int)x["Qty"],
                UnitPrice = (decimal)x["UnitPrice"]
            });
        return o;
    }

    /// <summary>Insert or update an open order together with its lines.</summary>
    public void Save(Order o)
    {
        using var c = Db.Open();
        using var tx = c.BeginTransaction();
        if (o.Id == 0)
        {
            o.OrderNo = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
            o.OrderDate = DateTime.Now;
            o.Id = Convert.ToInt32(Db.Scalar(c, tx,
                "INSERT INTO Orders(OrderNo,TableId,UserId,OrderDate,Status,Total) OUTPUT INSERTED.OrderId VALUES(@no,@t,@u,@d,'Open',0)",
                ("@no", o.OrderNo), ("@t", o.TableId), ("@u", o.UserId), ("@d", o.OrderDate)));
            Db.Exec(c, tx, "UPDATE CafeTables SET Status='Occupied' WHERE TableId=@t", ("@t", o.TableId));
        }
        else
        {
            Db.Exec(c, tx, "DELETE FROM OrderItems WHERE OrderId=@o", ("@o", o.Id));
        }

        foreach (var l in o.Lines)
            Db.Exec(c, tx, "INSERT INTO OrderItems(OrderId,ProductId,Qty,UnitPrice) VALUES(@o,@p,@q,@u)",
                ("@o", o.Id), ("@p", l.ProductId), ("@q", l.Qty), ("@u", l.UnitPrice));

        Db.Exec(c, tx, "UPDATE Orders SET Total=@t WHERE OrderId=@o", ("@t", o.Total), ("@o", o.Id));
        tx.Commit();
    }

    public Payment Pay(Order o, string method, decimal received)
    {
        var p = new Payment
        {
            OrderId = o.Id,
            Method = method,
            Amount = o.Total,
            Received = received,
            Change = Math.Max(0, received - o.Total),
            PaidDate = DateTime.Now
        };
        using var c = Db.Open();
        using var tx = c.BeginTransaction();
        p.Id = Convert.ToInt32(Db.Scalar(c, tx,
            "INSERT INTO Payments(OrderId,Method,Amount,Received,ChangeAmt,PaidDate) OUTPUT INSERTED.PaymentId VALUES(@o,@m,@a,@r,@c,@d)",
            ("@o", p.OrderId), ("@m", p.Method), ("@a", p.Amount), ("@r", p.Received), ("@c", p.Change), ("@d", p.PaidDate)));
        Db.Exec(c, tx, "UPDATE Orders SET Status='Paid', Total=@t WHERE OrderId=@o", ("@t", o.Total), ("@o", o.Id));
        Db.Exec(c, tx, "UPDATE CafeTables SET Status='Available' WHERE TableId=@t", ("@t", o.TableId));
        tx.Commit();
        o.Status = "Paid";
        return p;
    }

    public void Cancel(Order o)
    {
        using var c = Db.Open();
        using var tx = c.BeginTransaction();
        Db.Exec(c, tx, "UPDATE Orders SET Status='Cancelled' WHERE OrderId=@o", ("@o", o.Id));
        Db.Exec(c, tx, "UPDATE CafeTables SET Status='Available' WHERE TableId=@t", ("@t", o.TableId));
        tx.Commit();
        o.Status = "Cancelled";
    }

    public DataTable Search(DateTime from, DateTime to, string keyword, string status) =>
        Db.Query(@"SELECT o.OrderNo AS [Order No], t.TableName AS [Table], u.FullName AS Cashier,
                          o.OrderDate AS [Date], o.Status, o.Total
                   FROM Orders o
                   JOIN CafeTables t ON t.TableId=o.TableId
                   JOIN Users u ON u.UserId=o.UserId
                   WHERE o.OrderDate >= @f AND o.OrderDate < @t
                     AND (@s='' OR o.OrderNo LIKE '%'+@s+'%' OR u.FullName LIKE '%'+@s+'%' OR t.TableName LIKE '%'+@s+'%')
                     AND (@st='All' OR o.Status=@st)
                   ORDER BY o.OrderDate DESC",
            ("@f", from.Date), ("@t", to.Date.AddDays(1)), ("@s", keyword), ("@st", status));

    public DataTable GetItems(string orderNo) =>
        Db.Query(@"SELECT p.Name AS Product, i.Qty, i.UnitPrice AS Price, i.Qty*i.UnitPrice AS Total
                   FROM OrderItems i
                   JOIN Products p ON p.ProductId=i.ProductId
                   JOIN Orders o ON o.OrderId=i.OrderId
                   WHERE o.OrderNo=@no", ("@no", orderNo));
}
