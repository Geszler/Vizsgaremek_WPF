using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Vizsgaremek.ApiServices;

namespace Vizsgaremek
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new ApiClient();
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.CanResize;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string password = chkShowPassword.IsChecked == true
                ? txtPasswordVisible.Text
                : txtPassword.Password;

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Kérlek add meg a felhasználónevet és jelszót!", "Hiányzó adatok", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = await _client.LoginAsync(txtUsername.Text, password);

            if (success)
            {
                var gameSelection = new GameSelectionWindow(_client);
                gameSelection.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sikertelen bejelentkezés! Ellenőrizd az adatokat.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }
    }
}