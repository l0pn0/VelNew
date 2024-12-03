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
    /// Логика взаимодействия для NavigationPage.xaml
    /// </summary>
    public partial class NavigationPage : Page
    {
        public NavigationPage()
        {
            InitializeComponent();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Framesidebar_2.Navigate(new ClientUchPage());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Framesidebar_2.Navigate(new OrderPage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Framesidebar_2.Navigate(new Zakaz());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Framesidebar_2.Navigate(new ManageRentalPage());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.Framesidebar_2.Navigate(new Ren(this));
        }
    }
}
