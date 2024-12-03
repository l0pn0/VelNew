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
using System.Configuration;


namespace VelNew
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";
        private int selectedEquipmentId = -1;

        public OrderPage()
        {
            InitializeComponent();
            LoadEquipmentTypes();
            LoadEquipments();

        }
        private void LoadEquipmentTypes()
        {
            EquipmentTypeComboBox.Items.Add("Выберите Тип");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TypeName FROM EquipmentTypes";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    EquipmentTypeComboBox.Items.Add(reader.GetString(0));
                }
            }
            EquipmentTypeComboBox.SelectedIndex = 0;
        }

        private void LoadEquipments()
        {
            List<vw_Equipments> equipments = new List<vw_Equipments>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM vw_Equipments"; // Используем представление
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    equipments.Add(new vw_Equipments
                    {
                        EquipmentID = reader.GetInt32(reader.GetOrdinal("EquipmentID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        EquipmentTypeID = reader.GetInt32(reader.GetOrdinal("EquipmentTypeID")),
                        EquipmentType = reader.GetString(reader.GetOrdinal("EquipmentType")),
                        QuantityAvailable = reader.GetInt32(reader.GetOrdinal("QuantityAvailable")),
                        Condition = reader.GetString(reader.GetOrdinal("Condition")),
                        MaintenanceDate = reader.IsDBNull(reader.GetOrdinal("MaintenanceDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("MaintenanceDate")),
                        ShelfLife = reader.IsDBNull(reader.GetOrdinal("ShelfLife")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ShelfLife")),
                        IsReserved = reader.GetBoolean(reader.GetOrdinal("IsReserved"))
                    });
                }
            }

            EquipmentsDataGrid.ItemsSource = equipments;
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string equipmentType = EquipmentTypeComboBox.SelectedItem.ToString();
                    int equipmentTypeId = GetEquipmentTypeId(equipmentType);

                    string query = @"INSERT INTO Equipments 
                                     (EquipmentTypeID, Name, QuantityAvailable, Condition, MaintenanceDate, ShelfLife, IsReserved) 
                                     VALUES 
                                     (@EquipmentTypeID, @Name, @QuantityAvailable, @Condition, @MaintenanceDate, @ShelfLife, @IsReserved)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EquipmentTypeID", equipmentTypeId);
                    cmd.Parameters.AddWithValue("@Name", NameTextBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@QuantityAvailable", int.Parse(QuantityAvailableTextBox.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Condition", ConditionTextBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@MaintenanceDate", (object)MaintenanceDatePicker.SelectedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShelfLife", (object)ShelfLifeDatePicker.SelectedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsReserved", IsReservedCheckBox.IsChecked ?? false);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Инвентарь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEquipments();
                        ClearFields();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении инвентаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEquipmentId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите инвентарь для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ValidateInput())
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string equipmentType = EquipmentTypeComboBox.SelectedItem.ToString();
                    int equipmentTypeId = GetEquipmentTypeId(equipmentType);

                    string query = @"UPDATE Equipments 
                                     SET EquipmentTypeID=@EquipmentTypeID, Name=@Name, QuantityAvailable=@QuantityAvailable, 
                                         Condition=@Condition, MaintenanceDate=@MaintenanceDate, ShelfLife=@ShelfLife, 
                                         IsReserved=@IsReserved 
                                     WHERE EquipmentID=@EquipmentID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EquipmentTypeID", equipmentTypeId);
                    cmd.Parameters.AddWithValue("@Name", NameTextBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@QuantityAvailable", int.Parse(QuantityAvailableTextBox.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Condition", ConditionTextBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@MaintenanceDate", (object)MaintenanceDatePicker.SelectedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShelfLife", (object)ShelfLifeDatePicker.SelectedDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsReserved", IsReservedCheckBox.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@EquipmentID", selectedEquipmentId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Инвентарь успешно обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEquipments();
                        ClearFields();
                        selectedEquipmentId = -1;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении инвентаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEquipmentId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите инвентарь для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот инвентарь?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Equipments WHERE EquipmentID=@EquipmentID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EquipmentID", selectedEquipmentId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Инвентарь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEquipments();
                        ClearFields();
                        selectedEquipmentId = -1;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Ошибка при удалении инвентаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EquipmentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (EquipmentsDataGrid.SelectedItem is vw_Equipments selectedEquipment)
            {
                selectedEquipmentId = selectedEquipment.EquipmentID;
                NameTextBox.Text = selectedEquipment.Name;
                EquipmentTypeComboBox.SelectedItem = selectedEquipment.EquipmentType;
                QuantityAvailableTextBox.Text = selectedEquipment.QuantityAvailable.ToString();
                ConditionTextBox.Text = selectedEquipment.Condition;
                MaintenanceDatePicker.SelectedDate = selectedEquipment.MaintenanceDate;
                ShelfLifeDatePicker.SelectedDate = selectedEquipment.ShelfLife;
                IsReservedCheckBox.IsChecked = selectedEquipment.IsReserved;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название инвентаря.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EquipmentTypeComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите тип инвентаря.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(QuantityAvailableTextBox.Text.Trim(), out int quantity) || quantity < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество доступного инвентаря.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConditionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите состояние инвентаря.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            NameTextBox.Clear();
            EquipmentTypeComboBox.SelectedIndex = 0;
            QuantityAvailableTextBox.Clear();
            ConditionTextBox.Clear();
            MaintenanceDatePicker.SelectedDate = null;
            ShelfLifeDatePicker.SelectedDate = null;
            IsReservedCheckBox.IsChecked = false;
            EquipmentsDataGrid.SelectedItem = null;
            selectedEquipmentId = -1;
        }

        private int GetEquipmentTypeId(string typeName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT EquipmentTypeID FROM EquipmentTypes WHERE TypeName = @TypeName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TypeName", typeName);
                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    // Если тип инвентаря не найден, можно добавить его или обработать ошибку
                    throw new Exception("Тип инвентаря не найден.");
                }
            }

        }


    }
}

