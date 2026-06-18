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
    /// Логика взаимодействия для ChangeName.xaml
    /// </summary>
    public partial class ChangeName : Page
    {
        public ChangeName()
        {
            InitializeComponent();
            Info.Focus();
        }


        private void ClearText(object sender, RoutedEventArgs e)
        {
            WriteName.Clear();
            WriteName.Focus();
            WriteName.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Capybara(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetterOrDigit(c) || e.Text.Length > 5)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void NoSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter)
            {
                Changed.Visibility = Visibility.Visible;
                NamePl.Instance.PlayerName = WriteName.Text;
                Changed.Text = $"Имя успешно изменено на {NamePl.Instance.PlayerName}";
            }
            if (e.Key == Key.Escape)
            {
                MessageBox.Show($"Ваше текущее имя: {NamePl.Instance.PlayerName}");
                NavigationService.GoBack();

            }
        }

        private void ShortNick(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            
            if (textBox.Text.Length > 10)
            {
                textBox.Text = textBox.Text.Substring(0, 10);
                textBox.CaretIndex = 10;
            }
        }

        private void Goback(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBox.Show($"Ваше текущее имя: {NamePl.Instance.PlayerName}");
                NavigationService.GoBack();
            }
        }
    }
}
