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

        public Zakaz()
        {
            InitializeComponent();
            LoadStatusFilter();
            LoadOrders();
        }
        private void LoadStatusFilter()
        {
            StatusFilterComboBox.Items.Add("Все");
            StatusFilterComboBox.Items.Add("В ожидании");
            StatusFilterComboBox.Items.Add("Завершен");
            StatusFilterComboBox.Items.Add("Отменен");
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadOrders(string status = "Все", DateTime? startDate = null, DateTime? endDate = null)
        {
            List<Orders> orders = new List<Orders>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT o.OrderID, o.ClientID, o.OrderDate, o.TotalAmount, o.Status, 
                   CONCAT(c.FirstName, ' ', c.LastName) AS ClientFullName
            FROM Orders o
            INNER JOIN Clients c ON o.ClientID = c.ClientID
            WHERE (@Status = 'Все' OR o.Status = @Status)
            AND (@StartDate IS NULL OR o.OrderDate >= @StartDate)
            AND (@EndDate IS NULL OR o.OrderDate <= @EndDate)
            ORDER BY o.OrderDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = status;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = (object)startDate ?? DBNull.Value;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = (object)endDate ?? DBNull.Value;

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        orders.Add(new Orders
                        {
                            OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                            ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                            TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount"))
                                          ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                            // Не сохраняем имя клиента в Orders
                        });
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            OrdersDataGrid.ItemsSource = orders;

            // После загрузки заказов можно обновить DataGrid, чтобы показать имена клиентов
            UpdateClientNames(orders);
        }
        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ваш код для обработки изменения выбора
            var selectedOrder = OrdersDataGrid.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                // Дополнительные действия с выбранным заказом
                MessageBox.Show($"Выбрано: {selectedOrder.OrderID} - {selectedOrder.Status}");
            }
        }

        private void UpdateClientNames(List<Orders> orders)
        {
            foreach (var order in orders)
            {
                // Здесь можно получить имя клиента по ClientID,
                // например, сделав дополнительный запрос или используя уже загруженные данные.
                string clientName = GetClientNameById((int)order.ClientID);
                // Обновите отображение имени клиента в DataGrid, если это необходимо
                // Например, вы можете сделать это через DataGrid или дополнительный столбец
            }
        }
        private string GetClientNameById(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CONCAT(FirstName, ' ', LastName) AS FullName FROM Clients WHERE ClientID = @ClientID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                try
                {
                    conn.Open();
                    return cmd.ExecuteScalar() as string; // Возвращает полное имя клиента
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка при получении имени клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }

        private void FilterOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedStatus = StatusFilterComboBox.SelectedItem.ToString();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            LoadOrders(selectedStatus, startDate, endDate);
        }
    }

}

