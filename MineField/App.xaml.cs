using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace курсач
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // An explicit base URI makes embedded font lookup independent of the page.
            Resources["MinecraftFont"] = new System.Windows.Media.FontFamily(
                new Uri("pack://application:,,,/курсач;component/"), "./#ascii");
        }
    }
}
