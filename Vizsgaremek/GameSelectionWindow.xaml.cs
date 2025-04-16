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
    public partial class GameSelectionWindow : Window
    {
        private readonly ApiClient _client;

        public GameSelectionWindow(ApiClient client)
        {
            InitializeComponent();
            _client = client;
        }

        private void Wordle_Click(object sender, RoutedEventArgs e)
        {
            WordleWindow wordleWindow = new WordleWindow(_client);
            wordleWindow.Show();
            this.Close();
        }

        private void Snake_Click(object sender, RoutedEventArgs e)
        {
            SnakeWindow snakeWindow = new SnakeWindow(_client);
            snakeWindow.Show();
            this.Close();
        }

        private void FlappyBird_Click(object sender, RoutedEventArgs e)
        {
            FlappyBirdWindow game = new FlappyBirdWindow(_client);
            game.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Leaderboard_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardWindow leaderboardWindow = new LeaderboardWindow(_client);
            leaderboardWindow.Show();
            this.Close();
        }
    }
}
