using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using WembleyManagementSystem;



namespace LoginUser
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

        private readonly UserManagementSystem _userSystem;
        private readonly EventManagementSystem _eventSystem;

        //UserManagementSystem to use the method FindByCredentials, EventManagmentSystem to open the ClientForm
        public LoginForm(UserManagementSystem userSystem, EventManagementSystem eventSystem)
        {
            _userSystem = userSystem;
            _eventSystem = eventSystem;
            InitializeComponents();
        }

        //Property User logged
        public User loggedinUser { get; private set; }

        private void InitializeComponents()
        {
            this.Text = "Wembley Management System";
            this.Size = new Size(400, 390);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 252);

            // Thin gold accent strip at very top
            Panel accentStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Color.FromArgb(255, 190, 0)
            };

            // Main header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = Color.FromArgb(0, 55, 115)
            };
            Label lblHeaderTitle = new Label
            {
                Text = "WEMBLEY",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 12),
                Size = new Size(400, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Label lblHeaderSub = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 210, 255),
                Location = new Point(0, 50),
                Size = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(lblHeaderSub);

            // "Sign In" title
            lblTitle = new Label
            {
                Text = "Sign in to your account",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(60, 80, 110),
                Location = new Point(50, 110),
                Size = new Size(300, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Username
            lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 65, 90),
                Location = new Point(50, 148),
                Size = new Size(80, 18)
            };
            txtUsername = new TextBox
            {
                Location = new Point(50, 168),
                Size = new Size(295, 26),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 65, 90),
                Location = new Point(50, 206),
                Size = new Size(80, 18)
            };
            txtPassword = new TextBox
            {
                Location = new Point(50, 226),
                Size = new Size(295, 26),
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Login button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(50, 272),
                Size = new Size(295, 36),
                BackColor = Color.FromArgb(0, 55, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Separator line
            Panel separator = new Panel
            {
                Location = new Point(50, 320),
                Size = new Size(295, 1),
                BackColor = Color.FromArgb(210, 220, 235)
            };

            // Register button
            btnRegister = new Button
            {
                Text = "Don't have an account?  Create one",
                Location = new Point(50, 328),
                Size = new Size(295, 28),
                BackColor = Color.FromArgb(245, 247, 252),
                ForeColor = Color.FromArgb(0, 85, 170),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            this.Controls.Add(accentStrip);
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(separator);
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



            //Search in the list to match username and password

            loggedinUser = _userSystem.FindByCredentials(username, password);

            if (loggedinUser == null)
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                return;
            }

            MessageBox.Show($"Welcome back, {loggedinUser.Username}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear fields after login
            txtUsername.Clear();
            txtPassword.Clear();

            //Open the form based on the role
            switch (loggedinUser.UserRole)
            {
                case "Admin":
                case "Verified_Business":
                    // Open the shared Admin/Business dashboard
                    new AdminUser.AdminBusinessForm(_eventSystem, _userSystem, loggedinUser).Show();
                    break;

                case "Unverified_Business":
                    // Block access and show a message
                    MessageBox.Show("Your business account is pending verification by an Admin. You cannot access the dashboard yet.", 
                        "Account Unverified", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;

                case "Client":
                default:
                    new ClientForm(_eventSystem, _userSystem, loggedinUser.Username).Show();
                    break;
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            //Open a form to let the user choose the type of account to create

            Form choiceForm = new Form
            {
                Text = "Choose Account Type",
                Size = new Size(350, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label Question = new Label
            {
                Text = "What type of account do you want to create?",
                Location = new Point(30, 20),
                Size = new Size(290, 20),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            //CLient Button
            Button clientButton = new Button
            {
                Text = "Client Account",
                Location = new Point(50, 70),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            //Business Button
            Button businessButton = new Button
            {
                Text = "Business Account",
                Location = new Point(180, 70),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            //Open client registration form
            clientButton.Click += (s, args) =>
            {
                choiceForm.Close();
                RegisterUser.RegisterFormClient registerFormClient = new RegisterUser.RegisterFormClient(_userSystem);
                registerFormClient.ShowDialog();
                
            };

            //Open business registration form
            businessButton.Click += (s, args) =>
            {
                choiceForm.Close();
                RegisterFormBusiness registerFormBusiness = new RegisterFormBusiness(_userSystem);
                registerFormBusiness.ShowDialog();
            };

            choiceForm.Controls.Add(Question);
            choiceForm.Controls.Add(clientButton);
            choiceForm.Controls.Add(businessButton);
            choiceForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LoginForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "LoginForm";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.ResumeLayout(false);

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
