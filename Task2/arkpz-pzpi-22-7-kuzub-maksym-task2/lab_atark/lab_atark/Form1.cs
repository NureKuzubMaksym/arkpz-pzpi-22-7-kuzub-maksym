using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json.Linq;

namespace lab_atark
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async Task<double> GetDistanceAsync(string origin, string destination)
        {
            string apiKey = "AIzaSyCD0hjxCsMfNE_i7lYtioh4YOVZl0qL304";
            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&key={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Виконання запиту до API
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Перевірка на успішну відповідь

                    string responseBody = await response.Content.ReadAsStringAsync(); // Читання відповіді
                    JObject json = JObject.Parse(responseBody); // Розбір JSON відповіді

                    // Перевірка статусу відповіді
                    string status = (string)json["status"];
                    if (status != "OK")
                    {
                        MessageBox.Show($"Помилка API: {status}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 0;
                    }

                    // Перевірка наявності елементів (відстаней)
                    var elements = json["rows"][0]["elements"];
                    if (elements == null || elements.Count() == 0)
                    {
                        MessageBox.Show("Не вдалося знайти маршрут між адресами.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 0;
                    }

                    // Отримуємо статус елементів маршруту
                    string elementStatus = (string)elements[0]["status"];
                    if (elementStatus != "OK")
                    {
                        MessageBox.Show($"Помилка маршруту: {elementStatus}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 0;
                    }

                    // Отримання відстані у метрах
                    double distanceMeters = (double)elements[0]["distance"]["value"];
                    double distanceKilometers = distanceMeters / 1000.0; // Переводимо в кілометри

                    // Вивести відстань у label
                    label5.Text = $"Відстань: {distanceKilometers:F2} км";

                    return distanceKilometers;
                }
                catch (Exception ex)
                {
                    // Обробка помилок
                    MessageBox.Show($"Помилка при обчисленні відстані: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {// ід

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {// вантаж

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {// звідки

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {// куди

        }

        private async void button1_Click(object sender, EventArgs e)
        {// додати запис до бд
            string employeeIdText = textBox1.Text;
            string cargo = textBox2.Text;
            string fromAddress = textBox3.Text;
            string toAddress = textBox4.Text;

            if (string.IsNullOrEmpty(employeeIdText) || string.IsNullOrEmpty(cargo) ||
                string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(toAddress))
            {
                MessageBox.Show("Будь ласка, заповніть усі обов'язкові поля.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(employeeIdText, out int employeeId))
            {
                MessageBox.Show("ID працівника має бути числом.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обчислюємо відстань між точками
            double distance = await GetDistanceAsync(fromAddress, toAddress);

            if (distance == 0)
            {
                MessageBox.Show("Не вдалося обчислити відстань. Перевірте адреси.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обчислюємо витрати пального (наприклад, 8 літрів на 100 км)
            double fuelUsed = distance * 8 / 100;

            // Виводимо відстань та витрати пального в Label5
            label5.Text = $"Відстань: {distance:F2} км\nВитрати пального: {fuelUsed:F2} л";

            // Додаємо дані до бази даних
            string connectionString = "Server=DESKTOP-B751QVC;Database=atark;Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Trips (EmployeeId, Cargo, FromAddress, ToAddress, Distance, FuelUsed)
                        VALUES (@EmployeeId, @Cargo, @FromAddress, @ToAddress, @Distance, @FuelUsed)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeId", employeeId);
                        command.Parameters.AddWithValue("@Cargo", cargo);
                        command.Parameters.AddWithValue("@FromAddress", fromAddress);
                        command.Parameters.AddWithValue("@ToAddress", toAddress);
                        command.Parameters.AddWithValue("@Distance", distance);
                        command.Parameters.AddWithValue("@FuelUsed", fuelUsed);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Поїздку успішно додано до бази даних.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        textBox4.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Сталася помилка під час додавання даних у базу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        { //текстове поле з інфо про пальне та дистанцію

        }
    }
}
