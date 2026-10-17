namespace CafePOS;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        while (true)
        {
            using var login = new FormLogin();
            if (login.ShowDialog() != DialogResult.OK) break;

            using var main = new FormMain();
            Application.Run(main);
            if (!main.LoggedOut) break;
        }
    }
}
