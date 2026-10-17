using System.Drawing.Printing;
using System.Text;

namespace CafePOS;

public class FormReceipt : Form
{
    readonly TextBox txt = new()
    {
        Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10), ScrollBars = ScrollBars.Vertical, BackColor = Color.White
    };

    public FormReceipt(Order order, Payment pay)
    {
        Text = "Receipt";
        ClientSize = new Size(380, 560);
        StartPosition = FormStartPosition.CenterParent;

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        UiHelper.Btn(bottom, "Print", 80, 9, 100, (_, _) => Print());
        UiHelper.Btn(bottom, "Close", 200, 9, 100, (_, _) => Close());
        Controls.Add(txt);
        Controls.Add(bottom);

        txt.Text = Build(order, pay);
        txt.SelectionStart = 0;
        txt.SelectionLength = 0;
    }

    static string Build(Order o, Payment p)
    {
        const int w = 38;
        string line = new('-', w);
        var sb = new StringBuilder();
        sb.AppendLine("        CAFÉ POS");
        sb.AppendLine("     Thank you for visiting");
        sb.AppendLine(line);
        sb.AppendLine($"Order : {o.OrderNo}");
        sb.AppendLine($"Table : {o.TableName}");
        sb.AppendLine($"Date  : {p.PaidDate:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Cashier: {Session.CurrentUser?.FullName}");
        sb.AppendLine(line);
        sb.AppendLine($"{"Item",-18}{"Qty",3}{"Price",7}{"Total",9}");
        foreach (var l in o.Lines)
        {
            string name = l.ProductName.Length > 17 ? l.ProductName[..17] : l.ProductName;
            sb.AppendLine($"{name,-18}{l.Qty,3}{l.UnitPrice,7:N2}{l.Total,9:N2}");
        }
        sb.AppendLine(line);
        sb.AppendLine($"{"TOTAL",-27}{p.Amount,11:N2}");
        sb.AppendLine($"{"Paid (" + p.Method + ")",-27}{p.Received,11:N2}");
        sb.AppendLine($"{"Change",-27}{p.Change,11:N2}");
        sb.AppendLine(line);
        sb.AppendLine("      *** See you again ***");
        return sb.ToString();
    }

    void Print()
    {
        var pd = new PrintDocument();
        pd.PrintPage += (_, e) =>
            e.Graphics!.DrawString(txt.Text, txt.Font, Brushes.Black, e.MarginBounds);
        using var dlg = new PrintPreviewDialog { Document = pd, Width = 600, Height = 700 };
        dlg.ShowDialog(this);
    }
}
