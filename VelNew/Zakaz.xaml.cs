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
            List<OrderClientView> orders = new List<OrderClientView>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Подготовка запроса с фильтрами
                string query = @"
                SELECT * 
                FROM OrderClientView
                WHERE (@Status = 'Все' OR Status = @Status)
                AND (@StartDate IS NULL OR OrderDate >= @StartDate)
                AND (@EndDate IS NULL OR OrderDate <= @EndDate)";

                SqlCommand cmd = new SqlCommand(query, conn);
                // Установка параметров
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = status;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = (object)startDate ?? DBNull.Value;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = (object)endDate ?? DBNull.Value;

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        orders.Add(new OrderClientView
                        {
                            OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                            ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                            ClientFullName = reader.GetString(reader.GetOrdinal("ClientFullName")), // Получаем полное имя клиента
                            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                            TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount"))
                                          ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        });
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            OrdersDataGrid.ItemsSource = orders;
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранный заказ
            var selectedOrder = OrdersDataGrid.SelectedItem as OrderClientView;

            if (selectedOrder != null)
            {
                // Отображаем информацию о выбранном заказе
                MessageBox.Show($"Выбрано: {selectedOrder.OrderID} - {selectedOrder.ClientFullName} - {selectedOrder.Status}");
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

 