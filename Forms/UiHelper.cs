namespace CafePOS;

/// <summary>Small helpers so the forms stay short (all UI is built in code).</summary>
public static class UiHelper
{
    public static Label Lbl(Control p, string text, int x, int y, int w = 110)
    {
        var l = new Label { Text = text, Left = x, Top = y + 3, Width = w, AutoSize = false, Height = 22 };
        p.Controls.Add(l);
        return l;
    }

    public static TextBox Txt(Control p, int x, int y, int w = 200)
    {
        var t = new TextBox { Left = x, Top = y, Width = w };
        p.Controls.Add(t);
        return t;
    }

    public static Button Btn(Control p, string text, int x, int y, int w, EventHandler click, int h = 32)
    {
        var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = h };
        b.Click += click;
        p.Controls.Add(b);
        return b;
    }

    public static DataGridView Grid() => new()
    {
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = Color.White
    };

    public static void Error(string msg) =>
        MessageBox.Show(msg, "Café POS", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void Info(string msg) =>
        MessageBox.Show(msg, "Café POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
