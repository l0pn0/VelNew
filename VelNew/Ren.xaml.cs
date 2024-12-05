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
    /// Логика взаимодействия для Ren.xaml
    /// </summary>
    public partial class Ren : Page
    {
        private string connectionString = "Server=DENISMAK\\SQLEXPRESS;Database=CompVelo;Integrated Security=True;";

        private List<Equipments> Equipments = new List<Equipments>(); // Инициализация списка оборудования
        private List<Clients> Clients = new List<Clients>(); // Инициализация списка клиентов
        private Clients clients;

        public Ren()
        {
            InitializeComponent();
            LoadData(); // Загрузка данных при инициализации
        }

        public Ren(Clients clients)
        {
            this.clients = clients;
            InitializeComponent();
            LoadData(); // Загрузка данных при передаче клиента
        }

        private void LoadData()
        {
            // Загрузка данных оборудования
            Equipments = LoadEquipmentsFromDatabase();
            EquipmentComboBox.ItemsSource = Equipments;
            ReturnEquipmentComboBox.ItemsSource = Equipments;

            // Загрузка данных клиентов
            Clients = LoadClientsFromDatabase();
            ClientComboBox.ItemsSource = Clients;
        }

        private List<Equipments> LoadEquipmentsFromDatabase()
        {
            List<Equipments> equipments = new List<Equipments>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Equipments"; // Замените на ваш SQL-запрос
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Equipments equipment = new Equipments
                        {
                            EquipmentID = reader.GetInt32(reader.GetOrdinal("EquipmentID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            // Заполните остальные свойства
                        };
                        equipments.Add(equipment);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return equipments;
        }

        private List<Clients> LoadClientsFromDatabase()
        {
            List<Clients> clients = new List<Clients>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Clients"; // Замените на ваш SQL-запрос
                SqlCommand cmd = new SqlCommand(query, conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Clients client = new Clients
                        {
                            ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            // Заполните остальные свойства
                        };
                        clients.Add(client);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return clients;
        }

        private void IssueEquipment_Click(object sender, RoutedEventArgs e)
        {
            var selectedEquipment = EquipmentComboBox.SelectedItem as Equipments;
            var selectedClient = ClientComboBox.SelectedItem as Clients;
            var rentalStartDate = RentalStartDatePicker.SelectedDate;
            var rentalEndDate = RentalEndDatePicker.SelectedDate;

            if (selectedEquipment == null || selectedClient == null || rentalStartDate == null || rentalEndDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Создание нового заказа
            var order = new Orders
            {
                ClientID = selectedClient.ClientID,
                OrderDate = DateTime.Now,
                TotalAmount = 0, // Временное значение, будет обновлено позже
                Status = "В ожидании"
            };

            // Сохранение заказа в базу данных
            int orderId = SaveOrder(order); // Предположим, что SaveOrder возвращает OrderID

            var orderDetail = new OrderDetails
            {
                OrderID = orderId, // Убедитесь, что вы получили OrderID после сохранения
                EquipmentID = selectedEquipment.EquipmentID,
                Quantity = 1, // Временное значение, можно изменить при необходимости
                RentalStartDate = rentalStartDate.Value,
                RentalEndDate = rentalEndDate.Value,
                PricePerDay = CalculatePricePerDay(selectedEquipment), // Метод для расчета цены
                SubTotal = CalculateSubTotal(1, CalculatePricePerDay(selectedEquipment), rentalStartDate.Value, rentalEndDate.Value)
            };

            // Добавьте детали заказа в базу данных
            SaveOrderDetail(orderDetail); // Реализуйте метод SaveOrderDetail

            MessageBox.Show("Инвентарь выдан успешно.");
            CheckNotifications();
        }

        private void ReturnEquipment_Click(object sender, RoutedEventArgs e)
        {
            var selectedEquipment = ReturnEquipmentComboBox.SelectedItem as Equipments;

            if (selectedEquipment == null)
            {
                MessageBox.Show("Пожалуйста, выберите инвентарь для возврата.");
                return;
            }

            var overdue = CheckForOverdue(selectedEquipment.EquipmentID);
            if (overdue)
            {
                var penalty = CalculatePenalty(selectedEquipment.EquipmentID);
                MessageBox.Show($"Инвентарь возвращен с просрочкой. Штраф: {penalty}");
            }
            else
            {
                MessageBox.Show("Инвентарь возвращен успешно.");
            }

            CheckNotifications();
        }

        private bool CheckForOverdue(int equipmentID)
        {
            // Логика для проверки просроченного возврата
            return false; // Замените на реальную логику
        }

        private decimal CalculatePenalty(int equipmentID)
        {
            // Логика для вычисления штрафа
            return 0; // Замените на реальную логику
        }

        private decimal CalculatePricePerDay(Equipments equipment)
        {
            // Логика для вычисления цены за день
            return 10; // Пример фиксированной цены, замените на реальную логику
        }

        private decimal CalculateSubTotal(int quantity, decimal pricePerDay, DateTime startDate, DateTime endDate)
        {
            // Логика для вычисления общей суммы
            return quantity * pricePerDay * (endDate - startDate).Days;
        }

        private void CheckNotifications()
        {
            // Логика проверки уведомлений для клиентов о предстоящих сроках возврата
            NotificationsListBox.Items.Clear();

            foreach (var client in Clients)
            {
                // Проверка, есть ли просроченные возвраты
                var overdueRentals = GetOverdueRentals(client.ClientID);
                if (overdueRentals.Any())
                {
                    foreach (var rental in overdueRentals)
                    {
                        NotificationsListBox.Items.Add($"Клиент {client.LastName}: Просрочка по инвентарю {rental.Equipments}.");
                    }
                }
            }
        }

        private List<OrderDetails> GetOverdueRentals(int clientId)
        {
            // Здесь вы можете реализовать логику получения просроченных арендуемых записей
            return new List<OrderDetails>(); // Возвращаем список просроченных записей
        }

        private int SaveOrder(Orders order)
        {
            // Реализуйте логику сохранения заказа в базу данных и возвращайте OrderID
            return 1; // Временно возвращаем значение
        }

        private void SaveOrderDetail(OrderDetails orderDetail)
        {
            // Реализуйте логику сохранения деталей заказа в базу данных
        }
    }
}