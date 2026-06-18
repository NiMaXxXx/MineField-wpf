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
    /// Логика взаимодействия для Liders.xaml
    /// </summary>
    public partial class Liders : Page
    {
        public Liders()
        {
            InitializeComponent();
            Loaded += Leaders_Loaded;

        }

        private void Leaders_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTextFromFile();
        }

        private void GoBAqak(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void EnterM(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize += 3;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Gray;
                button.FocusVisualStyle = null;
            }
        }

        private void ExitM(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.FontSize -= 3;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.White;
                button.FocusVisualStyle = null;
            }
        }
        private void LoadTextFromFile()
        {
            
            string filePath = "Records.txt";

            try
            {
                
                string text = System.IO.File.ReadAllText(filePath);

                
                Leaders.Text = text;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
    }
}
