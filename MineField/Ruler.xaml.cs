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


namespace курсач
{
    /// <summary>
    /// Логика взаимодействия для Ruler.xaml
    /// </summary>
    public partial class Ruler : Page
    {
        public Ruler()
        {
            InitializeComponent();

            GoBack.Focus();
            
        }

        

        private void goback(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }   
        }

        private void GoBack_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize -= 3;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.White;
                button.FocusVisualStyle = null;
            }
        }

        private void RuleEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize += 3;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Gray;
                button.FocusVisualStyle = null;
            }
        }

        private void Backwar(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }
    }
}
