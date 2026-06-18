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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace курсач
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        
        
        public MainPage()
        {
            InitializeComponent();
            Main.Focus();
            
            
        }


        private void Click_Rules(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Ruler());
        }

        private void ExitEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize += 4;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Gray;
                button.FocusVisualStyle = null;
            }
        }

        private void ExitLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize -= 4;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.White;
                button.FocusVisualStyle = null;
                
            }
        }

        private void Exite(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayGame(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Game());
        }

        private void LetsChangeName(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangeName());
        }


        private void LoadLeaders(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Liders());
        }
    }
}
