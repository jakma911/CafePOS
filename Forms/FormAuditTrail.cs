namespace CafePOS;

public class FormAuditTrail : Form
{
    readonly AuditRepository repo = new();
    readonly DateTimePicker dtFrom = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly DateTimePicker dtTo = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly TextBox txtKeyword = new() { Width = 180 };
    readonly DataGridView grid = UiHelper.Grid();

    public FormAuditTrail()
    {
        Text = "Audit Trail";
        ClientSize = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 8, 0, 0) };
        dtFrom.Value = DateTime.Today.AddDays(-7);
        top.Controls.Add(new Label { Text = "From", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        top.Controls.Add(dtFrom);
        top.Controls.Add(new Label { Text = "To", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        top.Controls.Add(dtTo);
        top.Controls.Add(new Label { Text = "Keyword", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        top.Controls.Add(txtKeyword);
        var btn = new Button { Text = "Search", Width = 80 };
        btn.Click += (_, _) => DoSearch();
        top.Controls.Add(btn);

        grid.Dock = DockStyle.Fill;
        Controls.Add(grid);
        Controls.Add(top);
        AcceptButton = btn;
        Load += (_, _) => DoSearch();
    }

    void DoSearch()
    {
        try
        {
            grid.DataSource = repo.Search(dtFrom.Value, dtTo.Value, txtKeyword.Text.Trim());
            if (grid.Columns["Date"] != null) grid.Columns["Date"]!.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
