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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void EnterCl(object sender, RoutedEventArgs e)
        {
            var login = Login.Text;
            var pass = Password.Password;

            var user = Class1.db.Users.FirstOrDefault(i => i.PasswordHash == pass && i.Username == login);

            if (user != null)
            {
                Class1.LoggedUser = user;
                NavigationService.Navigate(new MainPage());

            }
            else
            {
                MessageBox.Show("Проверьте правильность введеных данных");
            }
        }
        private void Login_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Логика, которая выполняется при изменении текста
            string loginText = Login.Text;
            // Например, можно добавить валидацию или вывод информации
            // MessageBox.Show($"Вы ввели логин: {loginText}");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrderPage());
        }
    }
}
