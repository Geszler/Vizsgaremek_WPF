using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vizsgaremek.ApiServices;

namespace Vizsgaremek
{
    public partial class LeaderboardWindow : Window
    {
        private readonly ApiClient _client;
        private LoggedInUser? _currentUser;

        public LeaderboardWindow(ApiClient client)
        {
            InitializeComponent();
            _client = client;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUser = await _client.GetLoggedInUserAsync();
            App.CurrentUser = _currentUser;
            await RefreshLeaderboard();
        }

        private async Task RefreshLeaderboard()
        {
            try
            {
                var users = await _client.GetLeaderboardAsync();

                users = users.OrderByDescending(u => u.Score).ToList();

                for (int i = 0; i < users.Count; i++)
                {
                    users[i].Rank = i + 1;
                }

                LeaderboardList.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a ranglista betöltésekor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || !_currentUser.IsAdmin)
            {
                MessageBox.Show("Nincs jogosultságod törölni felhasználókat.");
                return;
            }

            if (sender is Button btn && btn.Tag is LoggedInUser userToDelete)
            {
                if (userToDelete.Username.ToLower() == "admin")
                {
                    MessageBox.Show("Az admin felhasználó nem törölhető.");
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Biztosan törlöd a felhasználót: {userToDelete.Username}?",
                    "Megerősítés",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    var success = await _client.DeleteUserAsync(userToDelete.Id);

                    if (success)
                    {
                        await RefreshLeaderboard();
                        MessageBox.Show("Felhasználó sikeresen törölve.", "Törlés", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("A törlés nem sikerült.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var menu = new GameSelectionWindow(_client);
            menu.Show();
            this.Close();
        }
    }
}