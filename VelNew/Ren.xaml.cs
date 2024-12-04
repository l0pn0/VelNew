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
    /// Логика взаимодействия для Ren.xaml
    /// </summary>
    public partial class Ren : Page
    {
        private List<Equipments> Equipments;
        private List<Clients> Clients;
        private Clients clients;

        public Ren()
        {
            InitializeComponent();

            LoadData();
        }

        public Ren(Clients clients)
        {
            this.clients = clients;
        }

        private void LoadData()
        {
            EquipmentComboBox.ItemsSource = Equipments;
            ReturnEquipmentComboBox.ItemsSource = Equipments;
            ClientComboBox.ItemsSource = Clients;
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

            // Добавьте заказ в базу данных (или в коллекцию)
            // Например, используя метод SaveOrder(order);

            // Создание детали заказа
            var orderDetail = new OrderDetails
            {
                OrderID = order.OrderID, // Убедитесь, что вы получили OrderID после сохранения
                EquipmentID = selectedEquipment.EquipmentID,
                Quantity = 1, // Временное значение, можно изменить при необходимости
                RentalStartDate = rentalStartDate.Value,
                RentalEndDate = rentalEndDate.Value,
                PricePerDay = CalculatePricePerDay(selectedEquipment), // Метод для расчета цены
                SubTotal = CalculateSubTotal(1, CalculatePricePerDay(selectedEquipment), rentalStartDate.Value, rentalEndDate.Value)
            };

            // Добавьте детали заказа в базу данных (или в коллекцию)
            // Например, используя метод SaveOrderDetail(orderDetail);

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

            // Логика возврата инвентаря
            // Например, обновление статуса заказа в базе данных

            // Проверка, есть ли просрочка
            var overdue = CheckForOverdue(selectedEquipment.EquipmentID);
            if (overdue)
            {
                // Рассчитать штраф
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
            // Логика проверки, есть ли просрочка для данного инвентаря
            // Например, из базы данных
            return false; // Возвращаем результат проверки
        }

        private decimal CalculatePenalty(int equipmentID)
        {
            // Логика расчета штрафа за просрочку
            return 0; // Возвращаем сумму штрафа
        }

        private decimal CalculatePricePerDay(Equipments equipment)
        {
            // Логика для вычисления цены за день
            return 10; // Пример фиксированной цены, замените на реальную логику
        }

        private decimal CalculateSubTotal(int quantity, decimal pricePerDay, DateTime startDate, DateTime endDate)
        {
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
            // Например, из базы данных или из списка
            return new List<OrderDetails>(); // Возвращаем список просроченных записей
        }
    }
}

