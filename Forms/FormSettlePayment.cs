namespace CafePOS;

public class FormSettlePayment : Form
{
    readonly Order order;
    readonly OrderRepository orders = new();
    readonly AuditRepository audit = new();
    readonly ComboBox cboMethod = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TextBox txtReceived;
    readonly Label lblChange = new() { Left = 150, Top = 140, Width = 200, Height = 24, Font = new Font("Segoe UI", 11, FontStyle.Bold) };

    public Payment? Payment { get; private set; }

    public FormSettlePayment(Order order)
    {
        this.order = order;
        Text = "Settle Payment";
        ClientSize = new Size(400, 270);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = $"Order {order.OrderNo}  |  Table {order.TableName}",
            Left = 20, Top = 15, Width = 360, Height = 22
        });
        Controls.Add(new Label
        {
            Text = $"Total: {order.Total:N2}",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Left = 20, Top = 45, Width = 360, Height = 40
        });

        UiHelper.Lbl(this, "Method", 20, 100);
        cboMethod.Left = 150; cboMethod.Top = 100; cboMethod.Width = 200;
        cboMethod.Items.AddRange(new object[] { "Cash", "ABA Pay", "KHQR", "Card" });
        cboMethod.SelectedIndex = 0;
        Controls.Add(cboMethod);

        UiHelper.Lbl(this, "Received", 20, 140 - 40 + 40);
        txtReceived = UiHelper.Txt(this, 150, 140, 200);
        lblChange.Top = 175;
        UiHelper.Lbl(this, "Change", 20, 175);
        Controls.Add(lblChange);

        var btnPay = UiHelper.Btn(this, "Pay", 150, 220, 95, (_, _) => DoPay());
        UiHelper.Btn(this, "Cancel", 255, 220, 95, (_, _) => { DialogResult = DialogResult.Cancel; Close(); });
        AcceptButton = btnPay;

        txtReceived.Text = order.Total.ToString("0.00");
        txtReceived.TextChanged += (_, _) => UpdateChange();
        cboMethod.SelectedIndexChanged += (_, _) =>
        {
            if (cboMethod.SelectedItem as string != "Cash") txtReceived.Text = order.Total.ToString("0.00");
        };
        UpdateChange();
    }

    decimal Received => decimal.TryParse(txtReceived.Text, out var v) ? v : 0;

    void UpdateChange() => lblChange.Text = $"{Math.Max(0, Received - order.Total):N2}";

    void DoPay()
    {
        if (Received < order.Total)
        {
            UiHelper.Error("Received amount is less than the total.");
            return;
        }
        try
        {
            Payment = orders.Pay(order, (string)cboMethod.SelectedItem!, Received);
            audit.Log("Payment", $"{order.OrderNo} paid {Payment.Amount:N2} by {Payment.Method}");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
