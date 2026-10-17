using System.Data;

namespace CafePOS;

public class FormSalesSearch : Form
{
    readonly OrderRepository repo = new();
    readonly DateTimePicker dtFrom = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly DateTimePicker dtTo = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly TextBox txtKeyword = new() { Width = 150 };
    readonly ComboBox cboStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    readonly DataGridView gridOrders = UiHelper.Grid();
    readonly DataGridView gridItems = UiHelper.Grid();
    readonly Label lblSummary = new() { Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    public FormSalesSearch()
    {
        Text = "Sales Search";
        ClientSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterParent;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 8, 0, 0) };
        dtFrom.Value = DateTime.Today.AddDays(-7);
        cboStatus.Items.AddRange(new object[] { "All", "Open", "Paid", "Cancelled" });
        cboStatus.SelectedIndex = 0;
        top.Controls.Add(new Label { Text = "From", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        top.Controls.Add(dtFrom);
        top.Controls.Add(new Label { Text = "To", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        top.Controls.Add(dtTo);
        top.Controls.Add(new Label { Text = "Keyword", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        top.Controls.Add(txtKeyword);
        top.Controls.Add(new Label { Text = "Status", AutoSize = true, Margin = new Padding(8, 6, 4, 0) });
        top.Controls.Add(cboStatus);
        var btn = new Button { Text = "Search", Width = 80 };
        btn.Click += (_, _) => DoSearch();
        top.Controls.Add(btn);

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 280 };
        gridOrders.Dock = DockStyle.Fill;
        gridItems.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(gridOrders);
        split.Panel2.Controls.Add(gridItems);

        Controls.Add(split);
        Controls.Add(lblSummary);
        Controls.Add(top);

        gridOrders.SelectionChanged += (_, _) => ShowItems();
        AcceptButton = btn;
        Load += (_, _) => DoSearch();
    }

    void DoSearch()
    {
        try
        {
            var dt = repo.Search(dtFrom.Value, dtTo.Value, txtKeyword.Text.Trim(), (string)cboStatus.SelectedItem!);
            gridOrders.DataSource = dt;
            if (gridOrders.Columns["Total"] != null) gridOrders.Columns["Total"]!.DefaultCellStyle.Format = "N2";
            decimal paid = dt.AsEnumerable().Where(r => (string)r["Status"] == "Paid").Sum(r => (decimal)r["Total"]);
            lblSummary.Text = $"Orders: {dt.Rows.Count}    Paid sales: {paid:N2}   ";
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }

    void ShowItems()
    {
        if (gridOrders.CurrentRow == null) { gridItems.DataSource = null; return; }
        var no = gridOrders.CurrentRow.Cells["Order No"].Value?.ToString();
        if (string.IsNullOrEmpty(no)) return;
        try { gridItems.DataSource = repo.GetItems(no); }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
