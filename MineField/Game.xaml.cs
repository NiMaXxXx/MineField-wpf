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

        public Game()
        {
            InitializeComponent();
            NameOfPlayer.Text = NamePl.Instance.PlayerName;

        }

        private void SpawnPlayer(object sender, RoutedEventArgs e)
        {
            gameInstance = new Gamy(gameCanvas, this);
            
        }
        public void ExitToMenu()
        {
            NavigationService.GoBack();
        }
        
    }

    public class Gamy
    {
        private List<Bomb> bombs;
        private Player player;
        private DispatcherTimer Gametimer = new DispatcherTimer(), invisibleTimer = new DispatcherTimer();
        private Canvas canv;
        private TextBlock timer, levelpl, textBlock = new TextBlock(), death = new TextBlock();
        private int previousLevel = 1;

        private Button resumeButton, exitButton;
        private string playername = NamePl.Instance.PlayerName;
        public event KeyEventHandler KeyDown;


        DispatcherTimer defaulttimer = new DispatcherTimer();
        private int elapsedTime = 0;
        private bool isPaused = false, IsSaved = false;
        private Game gamepage;
        public Gamy(Canvas gameCanvas, Game gamepage)
        {

            
            textBlock.FontSize = 28;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontFamily = new FontFamily("ascii");
            textBlock.Text = "Пауза";
            textBlock.Foreground = new SolidColorBrush((Colors.Gray));
            textBlock.Visibility = Visibility.Collapsed;

            death.FontSize = 28;
            death.Foreground = new SolidColorBrush(Colors.White);
            death.FontFamily = new FontFamily("ascii");
            death.Text = "Вы умерли!";
            death.Foreground = new SolidColorBrush((Colors.Gray));
            death.Visibility = Visibility.Collapsed;


            this.gamepage = gamepage;
            defaulttimer.Interval = TimeSpan.FromMilliseconds(1000);
            defaulttimer.Tick += Timer_Tick;
            defaulttimer.Start();

            invisibleTimer.Interval = TimeSpan.FromSeconds(3);
            invisibleTimer.Tick += InvisibleTimer_Tick;
            invisibleTimer.Start();


            this.canv = gameCanvas;
            canv.Focus();


            canv.KeyDown += Canvas_KeyDown;

            Canvas.SetLeft(textBlock, 610);
            Canvas.SetTop(textBlock, 245);
            canv.Children.Add(textBlock);

            Canvas.SetLeft(death, 600);
            Canvas.SetTop(death, 245);
            canv.Children.Add(death);



            player = new Player(608, 615, canv);
            bombs = new List<Bomb>();
            SpawnBombs();
            Gametimer.Interval = TimeSpan.FromMilliseconds(18);
            CompositionTarget.Rendering += Gametick;
            Gametimer.Start();

            timer = new TextBlock();
            timer.FontSize = 18;
            timer.Foreground = new SolidColorBrush(Colors.White);
            timer.FontFamily = new FontFamily("ascii");
            timer.Text = "Время: 0 сек";
            levelpl = new TextBlock();
            levelpl.FontSize = 18;
            levelpl.Foreground = new SolidColorBrush(Colors.White);
            levelpl.FontFamily = new FontFamily("ascii");
            levelpl.Text = "Уровень игрока: 1";

            Canvas.SetLeft(timer, 5);
            Canvas.SetTop(timer, 0);

            Canvas.SetLeft(levelpl, 5);
            Canvas.SetTop(levelpl, 25);
            canv.Children.Add(levelpl);
            canv.Children.Add(timer);

            resumeButton = new Button();
            resumeButton.Content = "Рестарт игры";
            resumeButton.Click += ResumeButton_Click;
            Canvas.SetLeft(resumeButton, 350);
            Canvas.SetTop(resumeButton, 300);
            resumeButton.Background = new SolidColorBrush(Colors.Transparent);
            resumeButton.FontFamily = new FontFamily("ascii");
            resumeButton.FontSize = 30;
            resumeButton.Foreground = new SolidColorBrush(Colors.Gray);
            resumeButton.BorderThickness = new Thickness(0, 0, 0, 0);
            canv.Children.Add(resumeButton);
            resumeButton.Visibility = Visibility.Hidden;

            exitButton = new Button();
            exitButton.Content = "Выйти";
            exitButton.Click += ExitButton_Click;
            Canvas.SetLeft(exitButton, 750);
            Canvas.SetTop(exitButton, 300);
            exitButton.Background = new SolidColorBrush(Colors.Transparent);
            exitButton.FontFamily = new FontFamily("ascii");
            exitButton.FontSize = 30;
            exitButton.Foreground = new SolidColorBrush(Colors.Gray);
            exitButton.BorderThickness = new Thickness(0, 0, 0, 0);
            canv.Children.Add(exitButton);
            exitButton.Visibility = Visibility.Hidden;



            if (!player.Playeralive())
            {
                defaulttimer.Stop();
            }

        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Escape)
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

            
            Gametimer.Stop();
            invisibleTimer.Stop();
            defaulttimer.Stop();

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
            
            Gametimer.Start();
            invisibleTimer.Start();
            defaulttimer.Start();
            elapsedTime = 0;
            player.SetLevelZero();

            textBlock.Visibility = Visibility.Collapsed;
            resumeButton.Visibility = Visibility.Collapsed;
            exitButton.Visibility = Visibility.Collapsed;
        }
        private void ResumeGame()
        {
            isPaused = false;
            Gametimer.Start();
            invisibleTimer.Start();
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
            if (!IsSaved) { SavePlayerInfo("Records.txt"); }
            gamepage.ExitToMenu();
        }



        private void InvisibleTimer_Tick(object sender, EventArgs e)
        {
            
            
            foreach (Bomb bomb in bombs)
            {
                bomb.MakeInvisible();
            }

            player.CanMove = true;
            
        }

        private void Gametick(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                player.Gametick(sender, e);
                player.CheckWin();
                
                foreach (Bomb bomb in bombs)
                {
                    player.CheckCollision(bomb);
                    if (!player.GetAlive())
                    {
                        bomb.MakeVisible();
                        if (!IsSaved)
                        {
                            SavePlayerInfo("Records.txt");
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
        public void SpawnBombs()
        {
            Random random = new Random();

            int numberOfBombs = random.Next(20, 30); 

            for (int i = 0; i < numberOfBombs; i++)
            {
                double x, y;
                do
                {
                    x = random.Next(0, (int)(canv.ActualWidth - 40)); 
                    y = random.Next(0, (int)(canv.ActualHeight - 40)); 
                    x += i * 20;
                    y += i * 20;
                } while (IsTooCloseToPlayer(x, y));
                

                

                Bomb bomb = new Bomb(x, y, canv);
                bombs.Add(bomb);
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

            
            if (player.Playeralive())
            {
                elapsedTime++;
                timer.Text = "Время: " + elapsedTime + " сек";
                levelpl.Text = "Уровень игрока: " + getlevel();

                
                if (player.pobeda)
                {
                    Canvas.SetLeft(player.playerRectangle, 608);
                    Canvas.SetTop(player.playerRectangle, 615);
                    
                    RestartGame();

                    previousLevel = getlevel();
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

        private void SavePlayerInfo(string filename)
        {
            try
            {
                string InfoAboutPlayer = $"{playername}   {player.GetLevelPL()}   {elapsedTime} сек ";

                List<string> existingLines = new List<string>();

                
                if (File.Exists(filename))
                {
                    existingLines = File.ReadAllLines(filename).ToList();
                }

                
                if (existingLines.Count >= 10)
                {
                    existingLines.RemoveAt(existingLines.Count - 1);
                }

                
                existingLines.Insert(0, InfoAboutPlayer);

                
                File.WriteAllLines(filename, existingLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось записать файл по причине {ex.Message}");
            }
        }
    }

    public class Object
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Image Image { get; protected set; }

        public Object(double x, double y, Canvas canvas)
        {
            X = x;
            Y = y;

            Image = new Image();
            Canvas.SetLeft(Image, x);
            Canvas.SetTop(Image, y);
        }
    }

    
    public class Player : Object
    {
        public Canvas playercan;
        protected bool isAlive = true, isExist = false;
        public bool CanMove = false;
        public Rectangle playerRectangle;
        private bool UpKeyPR, DownKeyPr, LeftKeyPr, RightKeyPr;
        public bool pobeda;
        private float Speed = 2;
        private int playerlevel = 1;
        private bool isPaused = false;

        public Player(double x, double y, Canvas can) : base(x, y, can)
        {
            this.playercan = can;
            
            playerRectangle = new Rectangle();
            playerRectangle.Width = 64;
            playerRectangle.Height = 64;
            playerRectangle.Fill = new ImageBrush(new BitmapImage(new Uri("C:/Users/яна солях/source/repos/курсач/курсач/kitiksmall.png")));

            
            Canvas.SetLeft(playerRectangle, x);
            Canvas.SetTop(playerRectangle, y);

            
            playercan.Children.Add(playerRectangle);

            playercan.KeyDown += PlayerCanvas_KeyDown;
            playercan.KeyUp += Playercan_KeyUp;


            playercan.Focusable = true;
            playercan.Focus();
        }

        public void IncreaseLevel(int num)
        {
            playerlevel = playerlevel + num;
        }

        public int GetLevelPL()
        {
            return playerlevel;
        }
        public bool Playeralive()
        {
            return isAlive;
        }
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
            if (e.Key == Key.Escape)
            {
                isPaused = !isPaused;
            }
        }
        public void Gametick(object sender, object e)
        {
            if (isAlive && CanMove)
            {
                
                if (UpKeyPR && Y > 0)
                {
                    Y -= Speed;
                }

                
                if (DownKeyPr && Y < playercan.ActualHeight - playerRectangle.Height - 40)
                {
                    Y += Speed;
                }

                
                if (LeftKeyPr && X > 0)
                {
                    X -= Speed;
                }

                
                if (RightKeyPr && X < playercan.ActualWidth - playerRectangle.Width - 8)
                {
                    X += Speed;
                }

                
                Canvas.SetLeft(playerRectangle, X);
                Canvas.SetTop(playerRectangle, Y);
                
                
            }
        }
        public void CheckCollision(Bomb bomb)
        {
            Rect playerRect = new Rect(X, Y, playerRectangle.Width, playerRectangle.Height);
            Rect bombRect = new Rect(bomb.X, bomb.Y, bomb.bombRectangle.Width - 16, bomb.bombRectangle.Height - 16);

            if (playerRect.IntersectsWith(bombRect))
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
        public bool GetAlive()
        {
            if (isAlive) return true;
            else return false;
        }

        internal void SetLevelZero()
        {
            playerlevel = 1;
        }
    }

    public class Bomb : Object
    {
        Canvas bombcan;
        public Rectangle bombRectangle;
        public Bomb(double x, double y, Canvas can) : base(x, y, can)
        {
            this.bombcan = can;
            
            bombRectangle = new Rectangle();
            bombRectangle.Width = 40; 
            bombRectangle.Height = 40;
            bombRectangle.Fill = new ImageBrush(new BitmapImage(new Uri("C:/Users/яна солях/source/repos/курсач/курсач/bimbasmall.png")));

            
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

