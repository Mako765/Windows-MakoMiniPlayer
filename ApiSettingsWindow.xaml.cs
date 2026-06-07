using System.Windows;

namespace MakoMiniPlayer
{
    public partial class ApiSettingsWindow : Window
    {
        private readonly AppSettings _settings;

        public ApiSettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            TxtOsApiKey.Text = settings.OsApiKey;
            TxtOsUsername.Text = settings.OsUsername;
            TxtOsPassword.Password = settings.OsPassword;
            TxtSubDLApiKey.Text = settings.SubDLApiKey;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _settings.OsApiKey = TxtOsApiKey.Text.Trim();
            _settings.OsUsername = TxtOsUsername.Text.Trim();
            _settings.OsPassword = TxtOsPassword.Password.Trim();
            _settings.SubDLApiKey = TxtSubDLApiKey.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}