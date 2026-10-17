namespace CafePOS;

public class FormProductList : Form
{
    readonly ProductRepository repo = new();
    readonly AuditRepository audit = new();
    readonly DataGridView grid = UiHelper.Grid();
    readonly TextBox txtName, txtCategory, txtPrice;
    readonly CheckBox chkActive = new() { Text = "Active", Left = 330, Top = 8, Checked = true };

    public FormProductList()
    {
        Text = "Products";
        ClientSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterParent;
        bool canEdit = Session.CurrentUser?.CanApprove ?? false;

        var top = new Panel { Dock = DockStyle.Top, Height = 90 };
        UiHelper.Lbl(top, "Name", 10, 8, 60);
        txtName = UiHelper.Txt(top, 70, 8, 240);
        UiHelper.Lbl(top, "Category", 10, 40, 60);
        txtCategory = UiHelper.Txt(top, 70, 40, 240);
        UiHelper.Lbl(top, "Price", 330, 40, 40);
        txtPrice = UiHelper.Txt(top, 375, 40, 90);
        top.Controls.Add(chkActive);

        var b1 = UiHelper.Btn(top, "Add", 490, 6, 80, (_, _) => Save(true), 28);
        var b2 = UiHelper.Btn(top, "Update", 575, 6, 80, (_, _) => Save(false), 28);
        var b3 = UiHelper.Btn(top, "Deactivate", 660, 6, 90, (_, _) => DeactivateProduct(), 28);
        UiHelper.Btn(top, "Refresh", 490, 40, 80, (_, _) => LoadData(), 28);
        foreach (var b in new[] { b1, b2, b3 }) b.Enabled = canEdit;
        if (!canEdit)
            top.Controls.Add(new Label { Text = "View only (Manager/Admin can edit)", Left = 575, Top = 46, Width = 180, ForeColor = Color.Gray });

        grid.Dock = DockStyle.Fill;
        grid.SelectionChanged += (_, _) => FillFields();
        Controls.Add(grid);
        Controls.Add(top);
        Load += (_, _) => LoadData();
    }

    void LoadData()
    {
        try { grid.DataSource = repo.GetAll(false); }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }

    Product? Current => grid.CurrentRow?.DataBoundItem as Product;

    void FillFields()
    {
        var p = Current;
        if (p == null) return;
        txtName.Text = p.Name;
        txtCategory.Text = p.Category;
        txtPrice.Text = p.Price.ToString("0.00");
        chkActive.Checked = p.IsActive;
    }

    void Save(bool isNew)
    {
        if (txtName.Text.Trim() == "" || txtCategory.Text.Trim() == "")
        { UiHelper.Error("Name and category are required."); return; }
        if (!decimal.TryParse(txtPrice.Text, out var price) || price < 0)
        { UiHelper.Error("Enter a valid price."); return; }
        try
        {
            if (isNew)
            {
                repo.Add(new Product { Name = txtName.Text.Trim(), Category = txtCategory.Text.Trim(), Price = price });
                audit.Log("Add Product", $"{txtName.Text.Trim()} @ {price:N2}");
            }
            else
            {
                var p = Current;
                if (p == null) return;
                p.Name = txtName.Text.Trim();
                p.Category = txtCategory.Text.Trim();
                p.Price = price;
                p.IsActive = chkActive.Checked;
                repo.Update(p);
                audit.Log("Update Product", $"#{p.Id} {p.Name} @ {p.Price:N2} active={p.IsActive}");
            }
            LoadData();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }

    void DeactivateProduct()
    {
        var p = Current;
        if (p == null) return;
        if (MessageBox.Show($"Deactivate '{p.Name}'?", "Café POS", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try
        {
            repo.Deactivate(p.Id);
            audit.Log("Deactivate Product", $"#{p.Id} {p.Name}");
            LoadData();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
