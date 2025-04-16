using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Vizsgaremek.ApiServices;

namespace Vizsgaremek
{
    public partial class RegisterWindow : Window
    {
        private readonly ApiClient _client = new ApiClient();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("A megadott jelszavak nem egyeznek!");
                return;
            }

            bool success = await _client.RegisterAsync(txtUsername.Text, txtPassword.Password);

            if (success)
            {
                MessageBox.Show("Sikeres regisztráció!");
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Hiba a regisztráció során!");
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;

            txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
            txtConfirmPassword.Visibility = Visibility.Collapsed;
            txtConfirmPasswordVisible.Visibility = Visibility.Visible;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;

            txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
            txtConfirmPassword.Visibility = Visibility.Visible;
            txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtPasswordVisible.Text = txtPassword.Password;
        }

        private void VisiblePassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtPassword.Password = txtPasswordVisible.Text;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtConfirmPasswordVisible.Text = txtConfirmPassword.Password;
        }

        private void VisibleConfirmPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtConfirmPassword.Password = txtConfirmPasswordVisible.Text;
        }
    }
}