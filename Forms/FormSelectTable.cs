namespace CafePOS;

public class FormSelectTable : Form
{
    readonly TableRepository repo = new();
    readonly FlowLayoutPanel panel = new() { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };
    public CafeTable? Selected { get; private set; }

    public FormSelectTable()
    {
        Text = "Select Table";
        ClientSize = new Size(620, 420);
        StartPosition = FormStartPosition.CenterParent;

        var top = new Panel { Dock = DockStyle.Top, Height = 44 };
        top.Controls.Add(new Label
        {
            Text = "Green = Available   |   Red = Occupied (open order)",
            Left = 12, Top = 12, Width = 380
        });
        UiHelper.Btn(top, "Refresh", 520, 6, 80, (_, _) => LoadTables(), 28);

        Controls.Add(panel);
        Controls.Add(top);
        Load += (_, _) => LoadTables();
    }

    void LoadTables()
    {
        panel.Controls.Clear();
        try
        {
            foreach (var t in repo.GetAll())
            {
                var b = new Button
                {
                    Text = $"{t.Name}\n{t.Status}",
                    Width = 130, Height = 90, Margin = new Padding(8),
                    BackColor = t.IsAvailable ? Color.LightGreen : Color.LightCoral,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Tag = t
                };
                b.Click += (_, _) =>
                {
                    Selected = t;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                panel.Controls.Add(b);
            }
        }
        catch (Exception ex) { UiHelper.Error(ex.Message); }
    }
}
