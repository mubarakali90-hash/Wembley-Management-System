using System;
using System.Drawing;
using System.Windows.Forms;

namespace user
{
    public class RegisterForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblUsername;
        private Label lblEmail;
        private Label lblPassword;
        private Label lblConfirmPassword;
        private Label lblTitle;

        public RegisterForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Register";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title Label
            lblTitle = new Label
            {
                Text = "Create Account",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(100, 20),
                Size = new Size(200, 35),
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

            // Email Label
            lblEmail = new Label
            {
                Text = "Email:",
                Location = new Point(50, 120),
                Size = new Size(80, 20)
            };

            // Email TextBox
            txtEmail = new TextBox
            {
                Location = new Point(140, 118),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10)
            };

            // Password Label
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(50, 160),
                Size = new Size(80, 20)
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Location = new Point(140, 158),
                Size = new Size(200, 25),
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10)
            };

            // Confirm Password Label
            lblConfirmPassword = new Label
            {
                Text = "Confirm:",
                Location = new Point(50, 200),
                Size = new Size(80, 20)
            };

            // Confirm Password TextBox
            txtConfirmPassword = new TextBox
            {
                Location = new Point(140, 198),
                Size = new Size(200, 25),
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10)
            };

            // Register Button
            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(140, 250),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(140, 295),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.Click += BtnCancel_Click;

            // Add controls to form
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(btnRegister);
            this.Controls.Add(btnCancel);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter an email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Save on json Text page
            MessageBox.Show($"Account created successfully!\nUsername: {username}\nEmail: {email}", 
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Close the registration form
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
