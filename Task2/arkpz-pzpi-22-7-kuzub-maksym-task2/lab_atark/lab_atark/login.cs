using System.Windows.Forms;
using System;
using System.Data.SqlClient;

namespace lab_atark
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {// Логін
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {// Пароль
        }

        private void login_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            // Рядок підключення до бази даних
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            string connectionString = "Server=DESKTOP-B751QVC;Database=atark;Trusted_Connection=True;";
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Запит до бази для перевірки логіну і паролю
                    string query = "SELECT IsAdmin FROM Users WHERE Username = @Username AND Password = @Password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            bool isAdmin = Convert.ToBoolean(result);

                            if (isAdmin)
                            {
                                // Відкрити форму 2 для адміністратора
                                Form2 adminForm = new Form2();
                                adminForm.Show();
                            }
                            else
                            {
                                // Відкрити форму 1 для користувача
                                Form1 userForm = new Form1();
                                userForm.Show();
                            }

                            // Закрити поточну форму
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Невірний логін або пароль!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка підключення до бази: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {//чисто щоб жилось краще
            // Якщо натиснуто Enter
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick(); // Емулюємо натискання кнопки
                e.SuppressKeyPress = true; // Відміняємо стандартну дію Enter
            }
        }

        private void login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

    }
}
