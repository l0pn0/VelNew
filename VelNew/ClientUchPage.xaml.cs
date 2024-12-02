using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
    /// Логика взаимодействия для ClientUchPage.xaml
    /// </summary>
    public partial class ClientUchPage : Page
    {

        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";
        private DataTable clientsTable;
        public ClientUchPage()
        {
            InitializeComponent();
            LoadClients();
        }



        private void LoadClients()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Clients", connection);
                clientsTable = new DataTable();
                adapter.Fill(clientsTable);
                ClientsListView.ItemsSource = clientsTable.DefaultView; // Привязываем данные к ListView
            }
        }

        // Метод для добавления нового клиента
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Clients (FirstName, LastName, PhoneNumber, Email, Address) VALUES (@FirstName, @LastName, @PhoneNumber, @Email, @Address)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                command.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                command.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                command.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                command.Parameters.AddWithValue("@Address", AddressTextBox.Text);

                connection.Open();
                command.ExecuteNonQuery(); // Выполняем команду добавления
            }
            LoadClients(); // Обновляем список клиентов
            ClearTextBoxes(); // Очищаем текстовые поля
        }

        // Метод для редактирования выбранного клиента
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListView.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ClientsListView.SelectedItem;
                int id = (int)selectedRow["ClientID"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Clients SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, Email = @Email, Address = @Address WHERE ClientID = @ClientID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                    command.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                    command.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                    command.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                    command.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                    command.Parameters.AddWithValue("@ClientID", id);

                    connection.Open();
                    command.ExecuteNonQuery(); // Выполняем команду обновления
                }
                LoadClients(); // Обновляем список клиентов
                ClearTextBoxes(); // Очищаем текстовые поля
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования."); // Сообщение об ошибке
            }
        }

        // Метод для удаления выбранного клиента
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListView.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ClientsListView.SelectedItem;
                int id = (int)selectedRow["ClientID"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Clients WHERE ClientID = @ClientID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClientID", id);

                    connection.Open();
                    command.ExecuteNonQuery(); // Выполняем команду удаления
                }
                LoadClients(); // Обновляем список клиентов
                ClearTextBoxes(); // Очищаем текстовые поля
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления."); // Сообщение об ошибке
            }
        }

        // Метод для очистки текстовых полей
        private void ClearTextBoxes()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            PhoneNumberTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Zakaz());
        }
    }
}
