using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ManageRentalPage.xaml
    /// </summary>
    public partial class ManageRentalPage : Page
    {
        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";
        private int selectedOrderId = -1;
        public ManageRentalPage()
        {
            InitializeComponent();
            LoadClients();
            LoadEquipments();
            LoadRentals();

        }
        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, CONCAT(FirstName, ' ', LastName) AS FullName FROM Clients";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ClientsComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader.GetString(reader.GetOrdinal("FullName")),
                        Tag = reader.GetInt32(reader.GetOrdinal("ClientID"))
                    });
                }
            }
        }

        private void LoadEquipments()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT EquipmentID, Name FROM Equipments WHERE QuantityAvailable > 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    EquipmentsComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader.GetString(reader.GetOrdinal("Name")),
                        Tag = reader.GetInt32(reader.GetOrdinal("EquipmentID"))
                    });
                }
            }
        }

        private void LoadRentals()
        {
            List<Reservations> reservations = new List<Reservations>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT r.ReservationID, CONCAT(c.FirstName, ' ', c.LastName) AS ClientName, e.Name AS EquipmentName,
                   r.ReservationDate, r.ReservedUntil
            FROM Reservations r
            INNER JOIN Clients c ON r.ClientID = c.ClientID
            INNER JOIN Equipments e ON r.EquipmentID = e.EquipmentID";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new Reservations
                    {
                        ReservationID = reader.GetInt32(reader.GetOrdinal("ReservationID")),
                        ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                        EquipmentID = reader.GetInt32(reader.GetOrdinal("EquipmentID")),
                        ReservationDate = reader.GetDateTime(reader.GetOrdinal("ReservationDate")),
                        ReservedUntil = reader.GetDateTime(reader.GetOrdinal("ReservedUntil"))
                    });
                }
            }

            RentalsDataGrid.ItemsSource = reservations;
        }

        private void IssueRentalButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsComboBox.SelectedItem == null || EquipmentsComboBox.SelectedItem == null ||
                !RentalEndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int clientId = (int)((ComboBoxItem)ClientsComboBox.SelectedItem).Tag;
            int equipmentId = (int)((ComboBoxItem)EquipmentsComboBox.SelectedItem).Tag;
            DateTime rentalStartDate = RentalStartDatePicker.SelectedDate.Value;
            DateTime rentalEndDate = RentalEndDatePicker.SelectedDate.Value;

            if (rentalEndDate < rentalStartDate)
            {
                MessageBox.Show("Дата окончания аренды не может быть раньше даты начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Создание заказа
                    string insertOrderQuery = @"
                        INSERT INTO Orders (ClientID, OrderDate, TotalAmount, Status)
                        VALUES (@ClientID, @OrderDate, @TotalAmount, @Status);
                        SELECT SCOPE_IDENTITY();";
                    SqlCommand insertOrderCmd = new SqlCommand(insertOrderQuery, conn, transaction);
                    insertOrderCmd.Parameters.AddWithValue("@ClientID", clientId);
                    insertOrderCmd.Parameters.AddWithValue("@OrderDate", rentalStartDate);
                    insertOrderCmd.Parameters.AddWithValue("@TotalAmount", 0); // Позже можно обновить
                    insertOrderCmd.Parameters.AddWithValue("@Status", "В ожидании");

                    int orderId = Convert.ToInt32(insertOrderCmd.ExecuteScalar());

                    // Создание детали заказа
                    string insertOrderDetailQuery = @"
                        INSERT INTO OrderDetails (OrderID, EquipmentID, Quantity, RentalStartDate, RentalEndDate, PricePerDay)
                        VALUES (@OrderID, @EquipmentID, @Quantity, @RentalStartDate, @RentalEndDate, @PricePerDay);
                        SELECT SCOPE_IDENTITY();";
                    SqlCommand insertOrderDetailCmd = new SqlCommand(insertOrderDetailQuery, conn, transaction);
                    insertOrderDetailCmd.Parameters.AddWithValue("@OrderID", orderId);
                    insertOrderDetailCmd.Parameters.AddWithValue("@EquipmentID", equipmentId);
                    insertOrderDetailCmd.Parameters.AddWithValue("@Quantity", 1); // Предположим, 1 единица
                    insertOrderDetailCmd.Parameters.AddWithValue("@RentalStartDate", rentalStartDate);
                    insertOrderDetailCmd.Parameters.AddWithValue("@RentalEndDate", rentalEndDate);
                    insertOrderDetailCmd.Parameters.AddWithValue("@PricePerDay", 100); // Примерная цена

                    int orderDetailId = Convert.ToInt32(insertOrderDetailCmd.ExecuteScalar());

                    // Обновление количества доступного инвентаря
                    string updateEquipmentQuery = @"
                        UPDATE Equipments 
                        SET QuantityAvailable = QuantityAvailable - 1, IsReserved = 1 
                        WHERE EquipmentID = @EquipmentID";
                    SqlCommand updateEquipmentCmd = new SqlCommand(updateEquipmentQuery, conn, transaction);
                    updateEquipmentCmd.Parameters.AddWithValue("@EquipmentID", equipmentId);
                    updateEquipmentCmd.ExecuteNonQuery();

                    // Обновление статуса заказа
                    string updateOrderStatusQuery = @"
                        UPDATE Orders 
                        SET Status = 'Выдан' 
                        WHERE OrderID = @OrderID";
                    SqlCommand updateOrderStatusCmd = new SqlCommand(updateOrderStatusQuery, conn, transaction);
                    updateOrderStatusCmd.Parameters.AddWithValue("@OrderID", orderId);
                    updateOrderStatusCmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Инвентарь успешно выдан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadRentals();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Ошибка при выдаче инвентаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReturnRentalButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для отметки возврата.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Обновление статуса заказа
                    string updateOrderStatusQuery = @"
                        UPDATE Orders 
                        SET Status = 'Завершен' 
                        WHERE OrderID = @OrderID";
                    SqlCommand updateOrderStatusCmd = new SqlCommand(updateOrderStatusQuery, conn, transaction);
                    updateOrderStatusCmd.Parameters.AddWithValue("@OrderID", selectedOrderId);
                    updateOrderStatusCmd.ExecuteNonQuery();

                    // Получение EquipmentID из OrderDetails
                    string getEquipmentIdQuery = @"
                        SELECT EquipmentID FROM OrderDetails 
                        WHERE OrderID = @OrderID";
                    SqlCommand getEquipmentIdCmd = new SqlCommand(getEquipmentIdQuery, conn, transaction);
                    getEquipmentIdCmd.Parameters.AddWithValue("@OrderID", selectedOrderId);
                    int equipmentId = Convert.ToInt32(getEquipmentIdCmd.ExecuteScalar());

                    // Обновление количества доступного инвентаря
                    string updateEquipmentQuery = @"
                        UPDATE Equipments 
                        SET QuantityAvailable = QuantityAvailable + 1, IsReserved = 0 
                        WHERE EquipmentID = @EquipmentID";
                    SqlCommand updateEquipmentCmd = new SqlCommand(updateEquipmentQuery, conn, transaction);
                    updateEquipmentCmd.Parameters.AddWithValue("@EquipmentID", equipmentId);
                    updateEquipmentCmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Возврат инвентаря успешно отмечен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadRentals();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Ошибка при отметке возврата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RentalsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RentalsDataGrid.SelectedItem is Reservations reservations)
            {
                selectedOrderId = reservations.ReservationID; // Присваиваем ReservationID
            }
        }

        private bool ValidateInput()
        {
            if (ClientsComboBox.SelectedItem == null ||
                EquipmentsComboBox.SelectedItem == null ||
                !RentalEndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }

}
