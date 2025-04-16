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
using System.Timers;
using System.Windows.Threading;
using Vizsgaremek.ApiServices;


namespace Vizsgaremek
{
    public partial class SnakeWindow : Window
    {
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Right;
        private Direction lastDirection = Direction.Right;

        private readonly List<Point> snakeParts = new();
        private Point foodPosition;
        private readonly Ellipse foodEllipse = new();

        private readonly int tileSize = 24;
        private int rows = 20, cols = 29;
        private int score;
        private readonly Random rand = new();

        private readonly Dictionary<Point, Rectangle> snakeRects = new();
        private int frameCounter = 0;
        private const int framesPerMove = 8;

        private readonly ApiClient _client;

        public SnakeWindow(ApiClient client)
        {
            InitializeComponent();
            _client = client;

            Loaded += SnakeWindow_Loaded;
            KeyDown += SnakeWindow_KeyDown;

            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }

        private void ReturnToGameSelection()
        {
            var menuWindow = new GameSelectionWindow(_client);
            menuWindow.Show();
            Close();
        }

        private void SnakeWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            DrawCheckeredBackground();
            StartButton.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
        }

        private void StartButton_Click(object? sender, RoutedEventArgs e)
        {
            StartGame();
            CompositionTarget.Rendering += GameLoop;
            GameCanvas.Focus();
        }

        private void DrawCheckeredBackground()
        {
            GameCanvas.Children.Clear();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rectangle tile = new()
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Fill = (x + y) % 2 == 0
                            ? new SolidColorBrush(Color.FromRgb(45, 45, 45))
                            : new SolidColorBrush(Color.FromRgb(60, 60, 60))
                    };
                    Canvas.SetLeft(tile, x * tileSize);
                    Canvas.SetTop(tile, y * tileSize);
                    GameCanvas.Children.Add(tile);
                }
            }
        }

        private void StartGame()
        {
            BackButton.Visibility = Visibility.Visible;
            StartButton.Visibility = Visibility.Collapsed;
            DrawCheckeredBackground();
            snakeParts.Clear();
            snakeRects.Clear();

            Point start = new(cols / 2, rows / 2);
            snakeParts.Add(start);
            AddSnakeRect(start, true);

            score = 0;
            ScoreText.Text = "Pontszám: 0";

            SpawnFood();
        }

        private void SpawnFood()
        {
            if (cols <= 0 || rows <= 0)
            {
                MessageBox.Show("A pálya mérete nem megfelelő.");
                return;
            }

            Point newFoodPosition;
            do
            {
                newFoodPosition = new Point(rand.Next(cols), rand.Next(rows));
            }
            while (snakeParts.Contains(newFoodPosition));

            foodPosition = newFoodPosition;

            Canvas.SetLeft(foodEllipse, foodPosition.X * tileSize);
            Canvas.SetTop(foodEllipse, foodPosition.Y * tileSize);

            foodEllipse.Width = tileSize;
            foodEllipse.Height = tileSize;
            foodEllipse.Fill = new SolidColorBrush(Color.FromRgb(255, 85, 100));

            if (!GameCanvas.Children.Contains(foodEllipse))
                GameCanvas.Children.Add(foodEllipse);
        }

        private void SnakeWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && lastDirection != Direction.Down)
                currentDirection = Direction.Up;
            else if (e.Key == Key.Down && lastDirection != Direction.Up)
                currentDirection = Direction.Down;
            else if (e.Key == Key.Left && lastDirection != Direction.Right)
                currentDirection = Direction.Left;
            else if (e.Key == Key.Right && lastDirection != Direction.Left)
                currentDirection = Direction.Right;
            else if (e.Key == Key.Escape)
                ReturnToGameSelection();
        }

        private async void GameLoop(object? sender, EventArgs e)
        {
            frameCounter++;
            if (frameCounter < framesPerMove)
                return;
            frameCounter = 0;

            Point head = snakeParts[0];

            switch (currentDirection)
            {
                case Direction.Up: head.Y--; break;
                case Direction.Down: head.Y++; break;
                case Direction.Left: head.X--; break;
                case Direction.Right: head.X++; break;
            }

            lastDirection = currentDirection;

            if (head.X < 0 || head.X >= cols || head.Y < 0 || head.Y >= rows || snakeParts.Contains(head))
            {
                CompositionTarget.Rendering -= GameLoop;
                MessageBox.Show($"Játék vége! Elért pontszám: {score}");

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
                    MessageBox.Show($"Hiba a pont mentésekor: {ex.Message}");
                }

                if (MessageBox.Show("Új játék indítása?", "Újrakezdés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StartGame();
                    CompositionTarget.Rendering += GameLoop;
                }
                else
                {
                    ReturnToGameSelection();
                }

                return;
            }

            snakeParts.Insert(0, head);

            foreach (var rect in snakeRects.Values)
            {
                GameCanvas.Children.Remove(rect);
            }
            snakeRects.Clear();

            for (int i = 0; i < snakeParts.Count; i++)
            {
                AddSnakeRect(snakeParts[i], i == 0);
            }

            if (head == foodPosition)
            {
                score++;
                ScoreText.Text = $"Pontszám: {score}";
                SpawnFood();
            }
            else
            {
                Point tail = snakeParts[^1];
                snakeParts.RemoveAt(snakeParts.Count - 1);
                RemoveSnakeRect(tail);
            }
        }

        private void AddSnakeRect(Point p, bool isHead = false)
        {
            Rectangle rect = new()
            {
                Width = tileSize,
                Height = tileSize,
                Fill = isHead
                    ? new SolidColorBrush(Color.FromRgb(0, 255, 180))
                    : new SolidColorBrush(Color.FromRgb(0, 180, 255)),
                RadiusX = 8,
                RadiusY = 8
            };
            Canvas.SetLeft(rect, p.X * tileSize);
            Canvas.SetTop(rect, p.Y * tileSize);
            if (!GameCanvas.Children.Contains(rect))
                GameCanvas.Children.Add(rect);
            snakeRects[p] = rect;
        }

        private void RemoveSnakeRect(Point p)
        {
            if (snakeRects.TryGetValue(p, out Rectangle? rect))
            {
                GameCanvas.Children.Remove(rect);
                snakeRects.Remove(p);
            }
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= GameLoop;
            ReturnToGameSelection();
        }
    }
}