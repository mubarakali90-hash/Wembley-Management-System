using System;
using System.Drawing;
using System.Windows.Forms;

namespace user
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblTitle;

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Login";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title Label
            lblTitle = new Label
            {
                Text = "User Login",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(120, 20),
                Size = new Size(160, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username Label
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(50, 80),
                Size = new Size(80, 20)
            };

            // Username TextBox
            txtUsername = new TextBox
            {
                Location = new Point(140, 78),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10)
            };

            // Password Label
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(50, 120),
                Size = new Size(80, 20)
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Location = new Point(140, 118),
                Size = new Size(200, 25),
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10)
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(140, 170),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Register Button
            btnRegister = new Button
            {
                Text = "Create Account",
                Location = new Point(140, 215),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(0, 120, 215),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnRegister.Click += BtnRegister_Click;

            // Add controls to form
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnRegister);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"Login successful!\nWelcome, {username}!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear fields after login
            txtUsername.Clear();
            txtPassword.Clear();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            // Open the registration form
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();

        }
    }
}
