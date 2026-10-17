namespace CafePOS;

/// <summary>A manager/admin must approve before an open order can be cancelled.</summary>
public class FormCancelOrder : Form
{
    readonly Order order;
    readonly UserRepository users = new();
    readonly OrderRepository orders = new();
    readonly AuditRepository audit = new();
    readonly TextBox txtReason, txtMgrUser, txtMgrPass;

    public FormCancelOrder(Order order)
    {
        this.order = order;
        Text = "Cancel Order - Approval";
        ClientSize = new Size(420, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = $"Cancel order {order.OrderNo}  (Table {order.TableName}, Total {order.Total:N2})",
            Left = 20, Top = 15, Width = 380, Height = 40
        });
        UiHelper.Lbl(this, "Reason", 20, 65);
        txtReason = UiHelper.Txt(this, 150, 65, 240);
        txtReason.Multiline = true;
        txtReason.Height = 60;

        Controls.Add(new Label
        {
            Text = "Manager / Admin approval required",
            Left = 20, Top = 145, Width = 300, Font = new Font("Segoe UI", 9, FontStyle.Bold)
        });
        UiHelper.Lbl(this, "Username", 20, 175);
        txtMgrUser = UiHelper.Txt(this, 150, 175, 240);
        UiHelper.Lbl(this, "Password", 20, 210);
        txtMgrPass = UiHelper.Txt(this, 150, 210, 240);
        txtMgrPass.UseSystemPasswordChar = true;

        var ok = UiHelper.Btn(this, "Approve & Cancel", 150, 260, 130, (_, _) => Approve());
        UiHelper.Btn(this, "Back", 290, 260, 100, (_, _) => { DialogResult = DialogResult.Cancel; Close(); });
        AcceptButton = ok;
    }

    void Approve()
    {
        var reason = txtReason.Text.Trim();
        if (reason == "") { UiHelper.Error("Please enter a reason."); return; }
        try
        {
            var approver = users.Authenticate(txtMgrUser.Text.Trim(), txtMgrPass.Text);
            if (approver == null || !approver.CanApprove)
            {
                UiHelper.Error("Approval denied: invalid credentials or the user is not a Manager/Admin.");
                audit.Log("Cancel Denied", $"{order.OrderNo} approval failed for '{txtMgrUser.Text.Trim()}'");
                return;
            }
            orders.Cancel(order);
            audit.Log("Cancel Order", $"{order.OrderNo} reason: {reason} | approved by {approver.Username}");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
