using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";
        private DataTable clientsTable;
        public OrderPage()
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
                    SELECT o.OrderID, CONCAT(c.FirstName, ' ', c.LastName) AS ClientName, o.OrderDate, o.TotalAmount, o.Status
                    FROM Orders o
                    INNER JOIN Clients c ON o.ClientID = c.ClientID
                    WHERE (@Status = 'Все' OR o.Status = @Status)
                      AND (@StartDate IS NULL OR o.OrderDate >= @StartDate)
                      AND (@EndDate IS NULL OR o.OrderDate <= @EndDate)
                    ORDER BY o.OrderDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                if (startDate.HasValue)
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                else
                    cmd.Parameters.AddWithValue("@StartDate", DBNull.Value);
                if (endDate.HasValue)
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
                else
                    cmd.Parameters.AddWithValue("@EndDate", DBNull.Value);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new Orders
                    {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        Status = reader.GetString(reader.GetOrdinal("Status"))
                    });
                }
            }

            OrdersDataGrid.ItemsSource = orders;
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

