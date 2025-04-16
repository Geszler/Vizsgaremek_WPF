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
using System.Windows.Threading;
using Vizsgaremek.ApiServices;

namespace Vizsgaremek
{
    public partial class WordleWindow : Window
    {
        private readonly ApiClient _client;
        private readonly List<string> WordList = new()
        {
            "APPLE", "HOUSE", "PLANE", "TRAIN", "TABLE",
            "CHAIR", "WATER", "BRICK", "GLASS", "LIGHT",
            "MOUSE", "PHONE", "CLOUD", "BREAD", "GREEN",
            "BLACK", "WHITE", "GRAPE", "SHOES", "SHEET",
            "CABLE", "BRAIN", "PLANT", "SUGAR", "WORLD",
            "HEART", "STONE", "SOUND", "HAPPY", "SLEEP",
            "MONEY", "CLOCK", "SMILE", "DREAM", "ROBOT"
        };

        private const int WordLength = 5;
        private const int MaxAttempts = 6;

        private string TargetWord = "";
        private List<string> Guesses = new();
        private int CurrentAttempt = 0;
        private int Score = 0;

        public WordleWindow(ApiClient client)
        {
            InitializeComponent();
            _client = client;

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;

            StartNewGame();
        }

        private void StartNewGame()
        {
            Random rnd = new();
            TargetWord = WordList[rnd.Next(WordList.Count)];
            Guesses.Clear();
            CurrentAttempt = 0;

            ScoreText.Text = $"Pontszám: {Score}";
            MessageText.Text = "";
            GuessInput.Text = "";
            GuessInput.IsEnabled = true;
            SubmitButton.IsEnabled = true;

            GameGrid.Children.Clear();

            for (int i = 0; i < MaxAttempts * WordLength; i++)
            {
                Border border = new()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(2),
                    Width = 55,
                    Height = 55,
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(10),
                    Background = Brushes.White,
                    Child = new TextBlock
                    {
                        Text = "",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                Grid.SetRow(border, i / WordLength);
                Grid.SetColumn(border, i % WordLength);
                GameGrid.Children.Add(border);
            }
        }

        private async void SubmitGuess()
        {
            string guess = GuessInput.Text.ToUpper().Trim();

            if (guess.Length != WordLength)
            {
                MessageBox.Show("A szó pontosan 5 betűs legyen!");
                return;
            }

            Guesses.Add(guess);

            for (int i = 0; i < WordLength; i++)
            {
                Border border = (Border)GameGrid.Children[CurrentAttempt * WordLength + i];
                TextBlock tb = (TextBlock)border.Child;
                tb.Text = guess[i].ToString();

                if (TargetWord[i] == guess[i])
                {
                    border.Background = Brushes.Green;
                    tb.Foreground = Brushes.White;
                }
                else if (TargetWord.Contains(guess[i]))
                {
                    border.Background = Brushes.Goldenrod;
                    tb.Foreground = Brushes.White;
                }
                else
                {
                    border.Background = Brushes.Gray;
                    tb.Foreground = Brushes.White;
                }
            }

            CurrentAttempt++;

            if (guess == TargetWord)
            {
                Score += 10;
                ScoreText.Text = $"Pontszám: {Score}";
                MessageText.Text = "🎉 Kitaláltad! Új játék indul...";

                GuessInput.IsEnabled = false;
                SubmitButton.IsEnabled = false;

                try
                {
                    if (!string.IsNullOrEmpty(_client.Token))
                    {
                        await _client.AddPointsAsync(10);
                    }
                    else
                    {
                        MessageBox.Show("⚠️ Nem vagy bejelentkezve. A pont nem került mentésre.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Hiba a pont mentésekor: {ex.Message}");
                }

                await Task.Delay(2000);
                StartNewGame();
            }
            else if (CurrentAttempt >= MaxAttempts)
            {
                MessageText.Text = $"❌ Játék vége! A szó: {TargetWord}";
                GuessInput.IsEnabled = false;
                SubmitButton.IsEnabled = false;
            }

            GuessInput.Text = "";
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitGuess();
        }

        private void GuessInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SubmitGuess();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = new GameSelectionWindow(_client);
            menu.Show();
            this.Close();
        }
    }
}
