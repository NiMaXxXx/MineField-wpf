using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace курсач
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {

        private Gamy gameInstance;
        private Window hostWindow;

        public Game()
        {
            InitializeComponent();
            NameOfPlayer.Text = NamePl.Instance.PlayerName;
            Unloaded += Game_Unloaded;

        }

        private void SpawnPlayer(object sender, RoutedEventArgs e)
        {
            if (gameInstance == null)
            {
                gameInstance = new Gamy(gameCanvas, this);
                hostWindow = Window.GetWindow(this);
                if (hostWindow != null)
                {
                    hostWindow.Deactivated += HostWindow_Deactivated;
                    hostWindow.Activated += HostWindow_Activated;
                    if (!hostWindow.IsActive) gameInstance.PauseForFocusLoss();
                }
            }
            
        }
        private void HostWindow_Deactivated(object sender, EventArgs e)
        {
            gameInstance?.PauseForFocusLoss();
        }

        private void HostWindow_Activated(object sender, EventArgs e)
        {
            // Return keyboard focus without automatically resuming the game.
            gameCanvas.Focus();
        }

        private void Game_Unloaded(object sender, RoutedEventArgs e)
        {
            if (hostWindow != null)
            {
                hostWindow.Deactivated -= HostWindow_Deactivated;
                hostWindow.Activated -= HostWindow_Activated;
                hostWindow = null;
            }
            if (gameInstance == null) return;
            gameInstance.Dispose();
            gameInstance = null;
            gameCanvas.Children.Clear();
        }

        public void ExitToMenu()
        {
            NavigationService.GoBack();
        }
        
    }

    public class Gamy : IDisposable
    {
        private List<Bomb> bombs;
        private Player player;
        private readonly DispatcherTimer invisibleTimer = new DispatcherTimer();
        private Canvas canv;
        private TimeSpan? previousRenderTime;
        private const double MaxFrameSeconds = 0.05;
        private TextBlock timer, levelpl, textBlock = new TextBlock(), death = new TextBlock();
        private const int MemorizationSeconds = 3;
        private int countdownRemaining;
        private readonly TextBlock countdown = new TextBlock();

        private Button resumeButton, exitButton;
        private string playername = NamePl.Instance.PlayerName;


        DispatcherTimer defaulttimer = new DispatcherTimer();
        private int elapsedTime = 0;
        private bool isPaused = false, IsSaved = false;
        private Game gamepage;
        public Gamy(Canvas gameCanvas, Game gamepage)
        {

            
            textBlock.FontSize = 28;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            textBlock.Text = "Пауза";
            textBlock.Foreground = new SolidColorBrush((Colors.Gray));
            textBlock.Visibility = Visibility.Collapsed;

            death.FontSize = 28;
            death.Foreground = new SolidColorBrush(Colors.White);
            death.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            death.Text = "Вы умерли!";
            death.Foreground = new SolidColorBrush((Colors.Gray));
            death.Visibility = Visibility.Collapsed;


            this.gamepage = gamepage;
            defaulttimer.Interval = TimeSpan.FromMilliseconds(1000);
            defaulttimer.Tick += Timer_Tick;
            defaulttimer.Start();

            invisibleTimer.Interval = TimeSpan.FromSeconds(1);
            invisibleTimer.Tick += InvisibleTimer_Tick;


            this.canv = gameCanvas;
            canv.Focus();


            canv.KeyDown += Canvas_KeyDown;

            Canvas.SetLeft(textBlock, 610);
            Canvas.SetTop(textBlock, 245);
            Panel.SetZIndex(textBlock, 100);
            canv.Children.Add(textBlock);

            Canvas.SetLeft(death, 600);
            Canvas.SetTop(death, 245);
            Panel.SetZIndex(death, 100);
            canv.Children.Add(death);

            countdown.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            countdown.FontSize = 26;
            countdown.Foreground = Brushes.White;
            countdown.Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
            countdown.TextAlignment = TextAlignment.Center;
            countdown.Padding = new Thickness(12, 6, 12, 6);
            countdown.Width = 500;
            Canvas.SetLeft(countdown, (canv.Width - countdown.Width) / 2);
            Canvas.SetTop(countdown, 80);
            Panel.SetZIndex(countdown, 100);
            canv.Children.Add(countdown);



            player = new Player(608, 615, canv);
            bombs = new List<Bomb>();
            SpawnBombs();
            BeginCountdown();
            CompositionTarget.Rendering += Gametick;

            timer = new TextBlock();
            timer.FontSize = 18;
            timer.Foreground = new SolidColorBrush(Colors.White);
            timer.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            timer.Text = "Время: 0 сек";
            levelpl = new TextBlock();
            levelpl.FontSize = 18;
            levelpl.Foreground = new SolidColorBrush(Colors.White);
            levelpl.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            levelpl.Text = "Уровень игрока: 1";

            Canvas.SetLeft(timer, 5);
            Canvas.SetTop(timer, 0);

            Canvas.SetLeft(levelpl, 5);
            Canvas.SetTop(levelpl, 25);
            Panel.SetZIndex(levelpl, 100);
            canv.Children.Add(levelpl);
            Panel.SetZIndex(timer, 100);
            canv.Children.Add(timer);

            resumeButton = new Button();
            resumeButton.Content = "Рестарт игры";
            resumeButton.Click += ResumeButton_Click;
            Canvas.SetLeft(resumeButton, 350);
            Canvas.SetTop(resumeButton, 300);
            resumeButton.Background = new SolidColorBrush(Colors.Transparent);
            resumeButton.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            resumeButton.FontSize = 30;
            resumeButton.Foreground = new SolidColorBrush(Colors.Gray);
            resumeButton.BorderThickness = new Thickness(0, 0, 0, 0);
            Panel.SetZIndex(resumeButton, 100);
            canv.Children.Add(resumeButton);
            resumeButton.Visibility = Visibility.Hidden;

            exitButton = new Button();
            exitButton.Content = "Выйти";
            exitButton.Click += ExitButton_Click;
            Canvas.SetLeft(exitButton, 750);
            Canvas.SetTop(exitButton, 300);
            exitButton.Background = new SolidColorBrush(Colors.Transparent);
            exitButton.FontFamily = (FontFamily)Application.Current.FindResource("MinecraftFont");
            exitButton.FontSize = 30;
            exitButton.Foreground = new SolidColorBrush(Colors.Gray);
            exitButton.BorderThickness = new Thickness(0, 0, 0, 0);
            Panel.SetZIndex(exitButton, 100);
            canv.Children.Add(exitButton);
            exitButton.Visibility = Visibility.Hidden;


        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= Gametick;
            invisibleTimer.Stop();
            defaulttimer.Stop();
            invisibleTimer.Tick -= InvisibleTimer_Tick;
            defaulttimer.Tick -= Timer_Tick;
            canv.KeyDown -= Canvas_KeyDown;
            player.DetachInput();
            resumeButton.Click -= ResumeButton_Click;
            exitButton.Click -= ExitButton_Click;
        }

        public void PauseForFocusLoss()
        {
            player.ResetInput();
            previousRenderTime = null;
            if (player.IsAlive && !isPaused) PauseGame();
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Escape && player.IsAlive)
            {
                
                
                if (!isPaused)
                {
                    PauseGame();
                    
                }
                else 
                {
                    ResumeGame();
                }
            }
        }

        private void PauseGame()
        {
            
            
            isPaused = true;
            player.ResetInput();

            
            invisibleTimer.Stop();
            defaulttimer.Stop();

            countdown.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;
            resumeButton.Visibility = Visibility.Visible;
            exitButton.Visibility = Visibility.Visible;
        }

        private void Restart()
        {

            isPaused = false;
            Canvas.SetLeft(player.playerRectangle, 608);
            Canvas.SetTop(player.playerRectangle, 615);
            RestartGamee();
            player.CanMove = false;
            player.ChangeLive();
            IsSaved = false;
            
            BeginCountdown();
            defaulttimer.Stop();
            defaulttimer.Start();
            elapsedTime = 0;
            player.SetLevelZero();
            player.pobeda = false;
            player.ResetInput();
            canv.Focus();
            timer.Text = "Время: 0 сек";
            levelpl.Text = "Уровень игрока: 1";
            death.Visibility = Visibility.Collapsed;

            textBlock.Visibility = Visibility.Collapsed;
            resumeButton.Visibility = Visibility.Collapsed;
            exitButton.Visibility = Visibility.Collapsed;
        }
        private void ResumeGame()
        {
            player.ResetInput();
            previousRenderTime = null;
            canv.Focus();
            isPaused = false;
            if (!player.CanMove)
            {
                countdown.Visibility = Visibility.Visible;
                invisibleTimer.Start();
            }
            defaulttimer.Start();
            resumeButton.Visibility = Visibility.Collapsed;
            exitButton.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Collapsed;
        }


        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            
            Restart();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSaved) { SavePlayerInfo(); }
            gamepage.ExitToMenu();
        }



        private void BeginCountdown()
        {
            player.CanMove = false;
            countdownRemaining = MemorizationSeconds;
            countdown.Text = $"Запомни мины: {countdownRemaining}";
            countdown.Visibility = Visibility.Visible;
            invisibleTimer.Stop();
            invisibleTimer.Start();
        }

        private void InvisibleTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused || !player.IsAlive) return;
            countdownRemaining--;
            if (countdownRemaining > 0)
            {
                countdown.Text = $"Запомни мины: {countdownRemaining}";
                return;
            }
            countdown.Visibility = Visibility.Collapsed;
            
            
            foreach (Bomb bomb in bombs)
            {
                bomb.MakeInvisible();
            }

            player.CanMove = true;
            invisibleTimer.Stop();
            
        }

        private void Gametick(object sender, EventArgs e)
        {
            var rendering = (RenderingEventArgs)e;
            TimeSpan currentTime = rendering.RenderingTime;
            double deltaSeconds = previousRenderTime.HasValue
                ? (currentTime - previousRenderTime.Value).TotalSeconds
                : 0;
            // Update even during pause so paused time never becomes movement.
            previousRenderTime = currentTime;
            if (deltaSeconds <= 0) return;
            // Avoid a large jump after a stalled frame.
            deltaSeconds = Math.Min(deltaSeconds, MaxFrameSeconds);

            if (!isPaused)
            {
                player.Move(deltaSeconds);
                player.CheckWin();
                
                foreach (Bomb bomb in bombs)
                {
                    player.CheckCollision(bomb);
                    if (!player.IsAlive)
                    {
                        bomb.MakeVisible();
                        if (!IsSaved)
                        {
                            SavePlayerInfo();
                            IsSaved = true;
                        }
                        death.Visibility = Visibility.Visible;
                        resumeButton.Visibility = Visibility.Visible;
                        exitButton.Visibility = Visibility.Visible;
                    }
                }
            }
            
            
            
        }

        public void RemoveBombs()
        {
            foreach (Bomb bomb in bombs)
            {
                bomb.RemoveFromCanvas();
            }

            bombs.Clear();
            
        }
        private const double MinimumBombCenterDistance = 120;
        private const int MaxSpawnAttemptsPerBomb = 1000;
        private readonly Random random = new Random();

        public void SpawnBombs()
        {
            int numberOfBombs = random.Next(20, 30);
            double maxX = canv.ActualWidth - Bomb.SpriteSize;
            double maxY = canv.ActualHeight - Bomb.SpriteSize;
            if (maxX < 0 || maxY < 0) return;

            for (int i = 0; i < numberOfBombs; i++)
            {
                for (int attempt = 0; attempt < MaxSpawnAttemptsPerBomb; attempt++)
                {
                    double x = random.NextDouble() * maxX;
                    double y = random.NextDouble() * maxY;
                    if (IsTooCloseToPlayer(x, y) || bombs.Any(bomb =>
                        (bomb.X - x) * (bomb.X - x) + (bomb.Y - y) * (bomb.Y - y)
                        < MinimumBombCenterDistance * MinimumBombCenterDistance))
                        continue;

                    bombs.Add(new Bomb(x, y, canv));
                    break;
                }
            }
        }
        private void RestartGame()
        {
            
            player.IncreaseLevel(1);

            
            player.X = 608;
            player.Y = 615;


            
            RemoveBombs();

            
            SpawnBombs();
        }
        private void RestartGamee()
        {
            

            
            player.X = 608;
            player.Y = 615;


            
            RemoveBombs();

            
            SpawnBombs();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            
            if (player.IsAlive)
            {
                elapsedTime++;
                timer.Text = "Время: " + elapsedTime + " сек";
                levelpl.Text = "Уровень игрока: " + getlevel();

                
                if (player.pobeda)
                {
                    Canvas.SetLeft(player.playerRectangle, 608);
                    Canvas.SetTop(player.playerRectangle, 615);
                    
                    RestartGame();
                    player.pobeda = false;
                    BeginCountdown();

                    player.CanMove = false;

                }
            }
        }

        public int getlevel()
        {
            return player.GetLevelPL();
        }
        private bool IsTooCloseToPlayer(double x, double y)
        {
            double distance = Math.Sqrt(Math.Pow(x - player.X, 2) + Math.Pow(y - player.Y, 2));
            return distance < 150;
        }

        private void SavePlayerInfo()
        {
            try
            {
                Leaderboard.Save(playername, player.GetLevelPL(), elapsedTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить рекорд: {ex.Message}");
            }
        }
    }

    public class Object
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Object(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    
    public class Player : Object
    {
        public Canvas playercan;
        private bool isAlive = true;
        public bool CanMove = false;
        public Rectangle playerRectangle;
        private bool UpKeyPR, DownKeyPr, LeftKeyPr, RightKeyPr;
        public bool pobeda;
        // WPF logical pixels per second, independent of the rendering frame rate.
        private const double Speed = 200;
        private int playerlevel = 1;

        public Player(double x, double y, Canvas can) : base(x, y)
        {
            this.playercan = can;
            
            playerRectangle = new Rectangle();
            playerRectangle.Width = 64;
            playerRectangle.Height = 64;
            playerRectangle.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/CalicoSprite.png")));

            
            Canvas.SetLeft(playerRectangle, x);
            Canvas.SetTop(playerRectangle, y);

            
            playercan.Children.Add(playerRectangle);

            playercan.KeyDown += PlayerCanvas_KeyDown;
            playercan.KeyUp += Playercan_KeyUp;


            playercan.Focusable = true;
            playercan.Focus();
        }

        public void ResetInput()
        {
            UpKeyPR = DownKeyPr = LeftKeyPr = RightKeyPr = false;
        }

        public void DetachInput()
        {
            playercan.KeyDown -= PlayerCanvas_KeyDown;
            playercan.KeyUp -= Playercan_KeyUp;
            ResetInput();
        }

        public void IncreaseLevel(int num)
        {
            playerlevel = playerlevel + num;
        }

        public int GetLevelPL()
        {
            return playerlevel;
        }
        public bool IsAlive => isAlive;
        public void ChangeLive()
        {
            isAlive = true;
        }

        private void Playercan_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                UpKeyPR = false;
            }
            if (e.Key == Key.A)
            {
                LeftKeyPr = false;
            }
            if (e.Key == Key.D)
            {
                RightKeyPr = false;
            }
            if (e.Key == Key.S)
            {
                DownKeyPr = false;
            }
            
        }

        public void PlayerCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                UpKeyPR = true;
            }
            if (e.Key == Key.A)
            {
                LeftKeyPr = true;
            }
            if (e.Key == Key.D)
            {
                RightKeyPr = true;
            }
            if (e.Key == Key.S)
            {
                DownKeyPr = true;
            }
        }
        public void Move(double deltaSeconds)
        {
            if (!isAlive || !CanMove || deltaSeconds <= 0) return;

            double directionX = (RightKeyPr ? 1 : 0) - (LeftKeyPr ? 1 : 0);
            double directionY = (DownKeyPr ? 1 : 0) - (UpKeyPR ? 1 : 0);
            double length = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (length == 0) return;

            // Normalize: diagonal movement must cover the same distance.
            double distance = Speed * deltaSeconds;
            X += directionX / length * distance;
            Y += directionY / length * distance;

            // Clamp after moving: a variable step can overshoot the edge.
            double maxX = Math.Max(0, playercan.ActualWidth - playerRectangle.Width - 8);
            double maxY = Math.Max(0, playercan.ActualHeight - playerRectangle.Height - 40);
            X = Math.Max(0, Math.Min(X, maxX));
            Y = Math.Max(0, Math.Min(Y, maxY));

            Canvas.SetLeft(playerRectangle, X);
            Canvas.SetTop(playerRectangle, Y);
        }
        // Smaller circular hitboxes allow harmless contact with sprite edges.
        private const double CollisionRadius = 16;

        public void CheckCollision(Bomb bomb)
        {
            double dx = X + playerRectangle.Width / 2 - (bomb.X + bomb.bombRectangle.Width / 2);
            double dy = Y + playerRectangle.Height / 2 - (bomb.Y + bomb.bombRectangle.Height / 2);
            double combinedRadius = CollisionRadius + Bomb.CollisionRadius;

            if (dx * dx + dy * dy < combinedRadius * combinedRadius)
            {
                isAlive = false;
                bomb.MakeVisible();
            }
        }
        public void CheckWin()
        {
            Rect playerRect = new Rect(X, Y, playerRectangle.Width, playerRectangle.Height);
            Rect winRect = new Rect(0, 0, 1280, 20); 

            if (playerRect.IntersectsWith(winRect))
            {
                pobeda = true;
            }
            else
            {
                pobeda = false;
            }
        }

        internal void SetLevelZero()
        {
            playerlevel = 1;
        }
    }

    public class Bomb : Object
    {
        public const double SpriteSize = 40;
        public const double CollisionRadius = 10;
        Canvas bombcan;
        public Rectangle bombRectangle;
        public Bomb(double x, double y, Canvas can) : base(x, y)
        {
            this.bombcan = can;
            
            bombRectangle = new Rectangle();
            bombRectangle.Width = SpriteSize;
            bombRectangle.Height = SpriteSize;
            bombRectangle.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/BombSprite.png")));

            
            Canvas.SetLeft(bombRectangle, x);
            Canvas.SetTop(bombRectangle, y);

            
            bombcan.Children.Add(bombRectangle);

        }

        public void MakeInvisible()
        {
            
            bombRectangle.Opacity = 0;
        }
        public void MakeVisible()
        {
            bombRectangle.Opacity = 100;
        }
        public void RemoveFromCanvas()
        {
            bombcan.Children.Remove(bombRectangle);
        }
    }
}

