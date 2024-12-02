using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Логика взаимодействия для Zakaz.xaml
    /// </summary>
    public partial class Zakaz : Page
    {
        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";
        private DataTable clientsTable;
        public Zakaz()
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

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListView.SelectedItem != null)
            {
                DataRowView selectedClient = (DataRowView)ClientsListView.SelectedItem;
                int clientId = (int)selectedClient["ClientID"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Orders (ClientID, EquipmentType, Quantity, RentalPeriodDays, ClientNotes, Status) VALUES (@ClientID, @EquipmentType, @Quantity, @RentalPeriodDays, @ClientNotes, @Status)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClientID", clientId);
                    command.Parameters.AddWithValue("@EquipmentType", EquipmentTypeTextBox.Text);
                    command.Parameters.AddWithValue("@Quantity", int.Parse(QuantityTextBox.Text));
                    command.Parameters.AddWithValue("@RentalPeriodDays", int.Parse(RentalPeriodTextBox.Text));
                    command.Parameters.AddWithValue("@ClientNotes", ClientNotesTextBox.Text);

                    // Проверка на выбранный статус
                    if (StatusComboBox.SelectedItem != null)
                    {
                        command.Parameters.AddWithValue("@Status", ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString());
                    }
                    else
                    {
                        MessageBox.Show("Пожалуйста, выберите статус заказа.");
                        return; // Выход из метода, если статус не выбран
                    }

                    connection.Open();
                    command.ExecuteNonQuery(); // Выполняем команду добавления
                }

                LoadOrders(clientId); // Загружаем заказы для выбранного клиента
                ClearOrderFields(); // Очищаем текстовые поля заказа
            }
            else
            {
                MessageBox.Show("Сначала выберите клиента для добавления заказа.");
            }
        }
        private void LoadOrders(int clientId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Orders WHERE ClientID = @ClientID";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@ClientID", clientId);
                DataTable ordersTable = new DataTable();
                adapter.Fill(ordersTable);
                OrdersListView.ItemsSource = ordersTable.DefaultView; // Привязываем данные к ListView
            }
        }
        private void ClientsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ClientsListView.SelectedItem != null)
            {
                DataRowView selectedClient = (DataRowView)ClientsListView.SelectedItem;
                int clientId = (int)selectedClient["ClientID"];
                LoadOrders(clientId); // Загружаем заказы для выбранного клиента
            }
        }
        private void ClearOrderFields()
        {
            EquipmentTypeTextBox.Clear();
            QuantityTextBox.Clear();
            RentalPeriodTextBox.Clear();
            ClientNotesTextBox.Clear();
            StatusComboBox.SelectedIndex = -1; // Сброс выбора
        }
    }
}
