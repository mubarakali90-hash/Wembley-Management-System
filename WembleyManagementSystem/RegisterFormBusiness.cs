using System;
using System.Drawing;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace WembleyManagementSystem
{
    public partial class RegisterFormBusiness : Form
    {
        private TextBox txtUsername;
        private TextBox txtBusinessName;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblBusinessName;
        private Label lblEmail;
        private Label lblPassword;
        private Label lblConfirmPassword;

        private readonly UserManagementSystem _userSystem;

        public RegisterFormBusiness(UserManagementSystem userSystem)
        {
            _userSystem = userSystem;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Business Registration";
            this.Size = new Size(400, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "Business Account",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(80, 20),
                Size = new Size(240, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username
            lblUsername = new Label { 
                Text = "Username:", Location = new Point(50, 80),
                Size = new Size(90, 20) };

            txtUsername = new TextBox
            { 
                Location = new Point(150, 78),
                Size = new Size(190, 25), 
                Font = new Font("Segoe UI", 10) 
            };

            // Business Name

            lblBusinessName = new Label 
            {
                Text = "Business:", Location = new Point(50, 120),
                Size = new Size(90, 20) 
            };

            txtBusinessName = new TextBox 
            {
                Location = new Point(150, 118),
                Size = new Size(190, 25),
                Font = new Font("Segoe UI", 10) 
            };

            // Email
            lblEmail = new Label 
            { 
                Text = "Email:", Location = new Point(50, 160),
                Size = new Size(90, 20) 
            };

            txtEmail = new TextBox
            { Location = new Point(150, 158),
                Size = new Size(190, 25),
                Font = new Font("Segoe UI", 10) 
            };

            // Password
            lblPassword = new Label 
            { 
                Text = "Password:", Location = new Point(50, 200),
                Size = new Size(90, 20) 
            };

            txtPassword = new TextBox 
            { Location = new Point(150, 198),
                Size = new Size(190, 25), PasswordChar = '•',
                Font = new Font("Segoe UI", 10) 
            };

            // Confirm Password
            lblConfirmPassword = new Label 
            { 
                Text = "Confirm:", Location = new Point(50, 240),
                Size = new Size(90, 20) 
            };

            txtConfirmPassword = new TextBox 
            {
                Location = new Point(150, 238), 
                Size = new Size(190, 25), PasswordChar = '•',
                Font = new Font("Segoe UI", 10)
            };

            // Register Button
            btnRegister = new Button
            {
                Text = "Register Business",
                Location = new Point(100, 290),
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
                Location = new Point(100, 335),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.Click += BtnCancel_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblBusinessName);
            this.Controls.Add(txtBusinessName);
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
            string businessName = txtBusinessName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter a username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(businessName))
            {
                MessageBox.Show("Please enter your business name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check username not already taken
            if (_userSystem.UsernameExists(username))
            {
                MessageBox.Show("That username is already taken. Please choose another.",
                    "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add user in the linked list and DB, role set to "Unverified_Business"
            User newUser = new User
            {
                UserID = 0,
                Username = username,
                Email = email,
                Password = password,
                UserRole = "Unverified_Business"
            };

            try
            {
                _userSystem.RegisterUser(newUser);

                MessageBox.Show($"Business account created!\nUsername: {username}\nBusiness: {businessName}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed:\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        

        private void RegisterFormBusiness_Load(object sender, EventArgs e)
        {

        }
    }


}