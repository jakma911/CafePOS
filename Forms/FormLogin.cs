namespace CafePOS;

public class FormLogin : Form
{
    readonly TextBox txtUser, txtPass;
    readonly UserRepository users = new();
    readonly AuditRepository audit = new();

    public FormLogin()
    {
        Text = "Café POS - Login";
        ClientSize = new Size(380, 230);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = "Café POS Management System",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            Left = 20, Top = 15, Width = 340, Height = 32
        });
        UiHelper.Lbl(this, "Username", 30, 65);
        txtUser = UiHelper.Txt(this, 140, 65, 200);
        UiHelper.Lbl(this, "Password", 30, 105);
        txtPass = UiHelper.Txt(this, 140, 105, 200);
        txtPass.UseSystemPasswordChar = true;

        var btnLogin = UiHelper.Btn(this, "Login", 140, 155, 95, (_, _) => DoLogin());
        UiHelper.Btn(this, "Exit", 245, 155, 95, (_, _) => { DialogResult = DialogResult.Cancel; Close(); });
        AcceptButton = btnLogin;

        Load += (_, _) =>
        {
            try { users.EnsureSeed(); }
            catch (Exception ex)
            {
                UiHelper.Error("Cannot connect to the database.\nCheck Db.ConnectionString (Data/Db.cs) and run Database/CafePOS.sql first.\n\n" + ex.Message);
            }
        };
    }

    void DoLogin()
    {
        try
        {
            var u = users.Authenticate(txtUser.Text.Trim(), txtPass.Text);
            if (u == null)
            {
                UiHelper.Error("Invalid username or password.");
                txtPass.Clear();
                txtPass.Focus();
                return;
            }
            Session.CurrentUser = u;
            audit.Log("Login", $"{u.Username} logged in");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
