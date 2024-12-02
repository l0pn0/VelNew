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

namespace VelNew
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private static MainPage _instance;
        public static MainPage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainPage();
                }
                return _instance;
            }
        }
        public MainPage()
        {
            InitializeComponent();
            _instance = this;
            Framesidebar_2.NavigationService.Navigate(new ClientUchPage());
            framesidebar.NavigationService.Navigate(new NavigationPage());
        }
    }
}
