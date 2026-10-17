namespace CafePOS;

public class FormMain : Form
{
    public bool LoggedOut { get; private set; }
    readonly AuditRepository audit = new();

    public FormMain()
    {
        var user = Session.CurrentUser!;
        Text = "Café POS - Main Menu";
        ClientSize = new Size(420, 430);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = "Café POS Management",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Left = 20, Top = 15, Width = 380, Height = 36
        });
        Controls.Add(new Label
        {
            Text = $"User: {user.FullName}  ({user.Role})",
            Left = 22, Top = 55, Width = 380, Height = 22
        });

        int y = 95;
        void Add(string text, Action act, bool enabled = true)
        {
            var b = UiHelper.Btn(this, text, 60, y, 300, (_, _) => act(), 44);
            b.Font = new Font("Segoe UI", 10.5f);
            b.Enabled = enabled;
            y += 54;
        }

        Add("New / Open Order", OpenOrder);
        Add("Products", () => { using var f = new FormProductList(); f.ShowDialog(this); });
        Add("Sales Search", () => { using var f = new FormSalesSearch(); f.ShowDialog(this); });
        Add("Audit Trail (Manager/Admin)", () => { using var f = new FormAuditTrail(); f.ShowDialog(this); }, user.CanApprove);
        Add("Logout", () =>
        {
            audit.Log("Logout", $"{user.Username} logged out");
            LoggedOut = true;
            Session.CurrentUser = null;
            Close();
        });
    }

    void OpenOrder()
    {
        using var sel = new FormSelectTable();
        if (sel.ShowDialog(this) != DialogResult.OK || sel.Selected == null) return;
        using var pos = new FormPosOrder(sel.Selected);
        pos.ShowDialog(this);
    }
}
