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
using System.Diagnostics;
using Vizsgaremek.ApiServices;


namespace Vizsgaremek
{
    public partial class FlappyBirdWindow : Window
    {
        private readonly ApiClient _client;
        private Rectangle bird = new();
        private List<Rectangle> pipes = new();
        private double gravity = 0.05;
        private double birdVelocity = 0;
        private int score = 0;
        private double pipeSpeed = 4;
        private double gapSize = 250;
        private Random rnd = new();
        private DateTime lastFrameTime = DateTime.Now;
        private double timeSinceLastPipe = 0;
        private double pipeInterval = 1.5;
        private List<Rectangle> scoredPipes = new();

        public FlappyBirdWindow(ApiClient client)
        {
            InitializeComponent();
            _client = client;

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;

            Loaded += Window_Loaded;
            CompositionTarget.Rendering += GameLoop;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gameCanvas.Width = ActualWidth;
            gameCanvas.Height = ActualHeight;
            StartGame();
        }

        private void StartGame()
        {
            score = 0;
            birdVelocity = 0;
            timeSinceLastPipe = 0;
            gameCanvas.Children.Clear();
            pipes.Clear();
            scoredPipes.Clear();

            bird = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.Yellow,
                RadiusX = 20,
                RadiusY = 20
            };

            Canvas.SetLeft(bird, gameCanvas.Width / 4);
            Canvas.SetTop(bird, gameCanvas.Height / 2);
            gameCanvas.Children.Add(bird);

            UpdateScore();
            CreatePipes();
        }

        private void CreatePipes()
        {
            double pipeWidth = 100;
            double canvasHeight = gameCanvas.Height;
            double pipeHeight = rnd.Next(100, (int)(canvasHeight - gapSize - 100));

            Rectangle topPipe = new()
            {
                Width = pipeWidth,
                Height = pipeHeight,
                Fill = Brushes.LimeGreen,
                RadiusX = 12,
                RadiusY = 12
            };

            Rectangle bottomPipe = new()
            {
                Width = pipeWidth,
                Height = canvasHeight - pipeHeight - gapSize,
                Fill = Brushes.LimeGreen,
                RadiusX = 12,
                RadiusY = 12
            };

            double startX = gameCanvas.Width;
            Canvas.SetLeft(topPipe, startX);
            Canvas.SetTop(topPipe, 0);
            Canvas.SetLeft(bottomPipe, startX);
            Canvas.SetTop(bottomPipe, pipeHeight + gapSize);

            pipes.Add(topPipe);
            pipes.Add(bottomPipe);
            scoredPipes.Add(bottomPipe);

            gameCanvas.Children.Add(topPipe);
            gameCanvas.Children.Add(bottomPipe);
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            double deltaTime = (DateTime.Now - lastFrameTime).TotalSeconds;
            lastFrameTime = DateTime.Now;
            timeSinceLastPipe += deltaTime;

            birdVelocity += gravity;
            Canvas.SetTop(bird, Canvas.GetTop(bird) + birdVelocity);

            for (int i = pipes.Count - 1; i >= 0; i--)
            {
                var pipe = pipes[i];
                double left = Canvas.GetLeft(pipe);
                Canvas.SetLeft(pipe, left - pipeSpeed * deltaTime * 60);

                if (left + pipe.Width < 0)
                {
                    gameCanvas.Children.Remove(pipe);
                    pipes.RemoveAt(i);
                }
            }

            for (int i = scoredPipes.Count - 1; i >= 0; i--)
            {
                var pipe = scoredPipes[i];
                if (Canvas.GetLeft(pipe) + pipe.Width < Canvas.GetLeft(bird))
                {
                    score++;
                    UpdateScore();
                    scoredPipes.RemoveAt(i);
                }
            }

            if (timeSinceLastPipe >= pipeInterval)
            {
                CreatePipes();
                timeSinceLastPipe = 0;
            }

            Rect birdHitBox = new(Canvas.GetLeft(bird), Canvas.GetTop(bird), bird.Width, bird.Height);
            foreach (var pipe in pipes)
            {
                Rect pipeHitBox = new(Canvas.GetLeft(pipe), Canvas.GetTop(pipe), pipe.Width, pipe.Height);
                if (birdHitBox.IntersectsWith(pipeHitBox))
                {
                    GameOver();
                    return;
                }
            }

            if (Canvas.GetTop(bird) < 0 || Canvas.GetTop(bird) + bird.Height > gameCanvas.Height)
            {
                GameOver();
            }
        }

        private void UpdateScore()
        {
            scoreLabel.Text = $"Pont: {score}";
        }

        private async void GameOver()
        {
            CompositionTarget.Rendering -= GameLoop;

            try
            {
                if (!string.IsNullOrEmpty(_client.Token))
                {
                    await _client.AddPointsAsync(score);
                }
                else
                {
                    MessageBox.Show("⚠️ Nem vagy bejelentkezve. A pont nem került mentésre.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a pont mentésekor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            var result = MessageBox.Show($"Vége a játéknak!\nPontszám: {score}\n\nSzeretnél újra játszani?", "Game Over", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                CompositionTarget.Rendering += GameLoop;
                StartGame();
            }
            else
            {
                GameSelectionWindow menu = new GameSelectionWindow(_client);
                menu.Show();
                this.Close();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Space)
            {
                birdVelocity = -3.5;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= GameLoop;
            GameSelectionWindow menu = new GameSelectionWindow(_client);
            menu.Show();
            this.Close();
        }
    }
}