using System.ComponentModel;

namespace CafePOS;

public class FormPosOrder : Form
{
    readonly CafeTable table;
    readonly ProductRepository products = new();
    readonly OrderRepository orders = new();
    readonly AuditRepository audit = new();

    List<Product> allProducts = new();
    Order order = new();
    BindingList<OrderLine> lines = new();

    readonly DataGridView gridProducts = UiHelper.Grid();
    readonly DataGridView gridLines = UiHelper.Grid();
    readonly TextBox txtSearch = new();
    readonly ComboBox cboCategory = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown numQty = new() { Minimum = 1, Maximum = 99, Value = 1, Width = 60 };
    readonly Label lblTotal = new()
    {
        Text = "Total: 0.00", Font = new Font("Segoe UI", 16, FontStyle.Bold),
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight
    };

    public FormPosOrder(CafeTable table)
    {
        this.table = table;
        Text = $"POS Order - {table.Name}";
        ClientSize = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterParent;

        // ---- left: products ----
        var left = new Panel { Dock = DockStyle.Left, Width = 470, Padding = new Padding(8) };
        var leftTop = new Panel { Dock = DockStyle.Top, Height = 70 };
        UiHelper.Lbl(leftTop, "Search", 0, 4, 55);
        txtSearch.Left = 60; txtSearch.Top = 4; txtSearch.Width = 160;
        leftTop.Controls.Add(txtSearch);
        UiHelper.Lbl(leftTop, "Category", 235, 4, 65);
        cboCategory.Left = 305; cboCategory.Top = 4; cboCategory.Width = 150;
        leftTop.Controls.Add(cboCategory);
        UiHelper.Lbl(leftTop, "Qty", 0, 36, 55);
        numQty.Left = 60; numQty.Top = 36;
        leftTop.Controls.Add(numQty);
        UiHelper.Btn(leftTop, "Add to order >>", 135, 34, 130, (_, _) => AddSelected(), 28);
        gridProducts.Dock = DockStyle.Fill;
        left.Controls.Add(gridProducts);
        left.Controls.Add(leftTop);

        // ---- right: order lines ----
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var rightTop = new Label
        {
            Text = $"Table: {table.Name}", Dock = DockStyle.Top, Height = 30,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        gridLines.Dock = DockStyle.Fill;
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 150 };
        lblTotal.Parent = bottom;
        lblTotal.Dock = DockStyle.Top; lblTotal.Height = 40;
        UiHelper.Btn(bottom, "Remove", 0, 50, 80, (_, _) => RemoveSelected());
        UiHelper.Btn(bottom, "+", 85, 50, 40, (_, _) => ChangeQty(1));
        UiHelper.Btn(bottom, "-", 130, 50, 40, (_, _) => ChangeQty(-1));
        UiHelper.Btn(bottom, "Save Order", 185, 50, 110, (_, _) => SaveOrder(true));
        UiHelper.Btn(bottom, "Settle Payment", 300, 50, 120, (_, _) => Settle());
        UiHelper.Btn(bottom, "Cancel Order", 425, 50, 100, (_, _) => CancelOrder());
        UiHelper.Btn(bottom, "Close", 0, 95, 80, (_, _) => Close());
        right.Controls.Add(gridLines);
        right.Controls.Add(rightTop);
        right.Controls.Add(bottom);

        Controls.Add(right);
        Controls.Add(left);

        txtSearch.TextChanged += (_, _) => FilterProducts();
        cboCategory.SelectedIndexChanged += (_, _) => FilterProducts();
        gridProducts.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) AddSelected(); };
        Load += (_, _) => Init();
    }

    void Init()
    {
        try
        {
            allProducts = products.GetAll(true);
            cboCategory.Items.Add("All");
            foreach (var c in allProducts.Select(p => p.Category).Distinct().OrderBy(c => c))
                cboCategory.Items.Add(c);
            cboCategory.SelectedIndex = 0;

            order = orders.GetOpenOrder(table.Id) ?? new Order
            {
                TableId = table.Id,
                TableName = table.Name,
                UserId = Session.CurrentUser!.Id
            };
            lines = new BindingList<OrderLine>(order.Lines);
            gridLines.DataSource = lines;
            gridLines.Columns["ProductId"]!.Visible = false;
            UpdateTotal();
            FilterProducts();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }

    void FilterProducts()
    {
        var kw = txtSearch.Text.Trim();
        var cat = cboCategory.SelectedItem as string ?? "All";
        var list = allProducts
            .Where(p => (cat == "All" || p.Category == cat) &&
                        (kw == "" || p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .Select(p => new { p.Id, p.Name, p.Category, p.Price })
            .ToList();
        gridProducts.DataSource = list;
        if (gridProducts.Columns["Id"] != null) gridProducts.Columns["Id"]!.Visible = false;
    }

    void UpdateTotal()
    {
        lblTotal.Text = $"Total: {order.Total:N2}";
        gridLines.Refresh();
    }

    void AddSelected()
    {
        if (gridProducts.CurrentRow == null) return;
        int id = (int)gridProducts.CurrentRow.Cells["Id"].Value;
        var p = allProducts.First(x => x.Id == id);
        var existing = lines.FirstOrDefault(l => l.ProductId == p.Id);
        if (existing != null) existing.Qty += (int)numQty.Value;
        else lines.Add(new OrderLine { ProductId = p.Id, ProductName = p.Name, Qty = (int)numQty.Value, UnitPrice = p.Price });
        lines.ResetBindings();
        numQty.Value = 1;
        UpdateTotal();
    }

    OrderLine? CurrentLine() => gridLines.CurrentRow?.DataBoundItem as OrderLine;

    void RemoveSelected()
    {
        var l = CurrentLine();
        if (l == null) return;
        lines.Remove(l);
        UpdateTotal();
    }

    void ChangeQty(int delta)
    {
        var l = CurrentLine();
        if (l == null) return;
        l.Qty += delta;
        if (l.Qty <= 0) lines.Remove(l);
        lines.ResetBindings();
        UpdateTotal();
    }

    bool SaveOrder(bool showMessage)
    {
        if (lines.Count == 0) { UiHelper.Error("The order is empty."); return false; }
        try
        {
            bool isNew = order.Id == 0;
            orders.Save(order);
            audit.Log(isNew ? "Create Order" : "Update Order", $"{order.OrderNo} table {table.Name} total {order.Total:N2}");
            if (showMessage) UiHelper.Info($"Order {order.OrderNo} saved.");
            return true;
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); return false; }
    }

    void Settle()
    {
        if (!SaveOrder(false)) return;
        using var pay = new FormSettlePayment(order);
        if (pay.ShowDialog(this) != DialogResult.OK || pay.Payment == null) return;
        using var rec = new FormReceipt(order, pay.Payment);
        rec.ShowDialog(this);
        DialogResult = DialogResult.OK;
        Close();
    }

    void CancelOrder()
    {
        if (order.Id == 0)
        {
            // nothing saved yet - just discard
            if (MessageBox.Show("Discard this unsaved order?", "Café POS", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Close();
            return;
        }
        using var f = new FormCancelOrder(order);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
