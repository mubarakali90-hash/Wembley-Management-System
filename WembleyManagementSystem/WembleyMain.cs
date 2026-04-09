using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace WembleyManagementSystem
{

    //Weather Api 
    public class WeatherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Api Key Hardcoded for testing
        private const string API_KEY = "617cf1f96800ece567d9af5cff8c364e";

        // Wembley Stadium coordinates
        private const double WEMBLEY_LAT = 51.556;
        private const double WEMBLEY_LON = -0.2796;

        //Get current weather for Wembley Stadium
        //Result temperature, weather description and icon
        public static async Task<WeatherResult> GetCurrentWeatherAsync()
        {
            try
            {

                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={WEMBLEY_LAT}&lon={WEMBLEY_LON}&appid={API_KEY}&units=metric";
                string response = await _httpClient.GetStringAsync(url);

                //Parsing json response
                JsonDocument json = JsonDocument.Parse(response);

                //Get the number after temp in the json response
                double temp = json.RootElement.GetProperty("main").GetProperty("temp").GetDouble();
                //Get  description in the json response
                string description = json.RootElement.GetProperty("weather")[0].GetProperty("description").GetString();
                //Get the string after icon in the json response in this case it's the icon code
                string icon = json.RootElement.GetProperty("weather")[0].GetProperty("icon").GetString();

                //object to hold the result and if the api call was successful
                return new WeatherResult
                {
                    Temperature = Math.Round(temp, 1),
                    Description = description,
                    IconCode = icon,
                    Success = true
                };
            }
            //In case of any error return a failed result
            catch (Exception ex)
            {
                Console.WriteLine($"Weather API error: {ex.Message}");
                return new WeatherResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }

    //Struct for results from the weather api
    public class WeatherResult
    {
        public double Temperature { get; set; }
        public string Description { get; set; }
        public string IconCode { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public string ToSummary()
        {
            if (!Success) return "Weather unavailable";
            return $"{Temperature}°C - {Description}";
        }
    }

    //UI
    public class PurchaseConfirmationForm : Form
    {
        public PurchaseConfirmationForm()
        {
            this.Text = "Purchase Successful";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;

            Label msg = new Label()
            {
                Text = "Thank you for your purchase!",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            this.Controls.Add(msg);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PurchaseConfirmationForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "PurchaseConfirmationForm";
            this.Load += new System.EventHandler(this.PurchaseConfirmationForm_Load);
            this.ResumeLayout(false);

        }

        private void PurchaseConfirmationForm_Load(object sender, EventArgs e)
        {

        }
    }
    //Form to show the tickets purchased by the user
    public class MyTicketsForm : Form
    {
        public MyTicketsForm(Purchase[] purchases, string username)
        {
            this.Text = $"My Tickets - {username}";
            this.Size = new Size(650, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 252);

            Panel accentStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Color.FromArgb(255, 190, 0)
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(0, 55, 115)
            };
            Label lblHeader = new Label
            {
                Text = $"  My Tickets ({purchases.Length})",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblHeader);

            DataGridView dgvTickets = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(225, 230, 240)
            };

            dgvTickets.EnableHeadersVisualStyles = false;
            dgvTickets.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 55, 115);
            dgvTickets.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTickets.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvTickets.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvTickets.ColumnHeadersHeight = 34;
            dgvTickets.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 246, 255);
            dgvTickets.DefaultCellStyle.Padding = new Padding(6, 3, 6, 3);
            dgvTickets.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgvTickets.DataSource = purchases;

            if (purchases.Length == 0)
            {
                Label lblNoTickets = new Label
                {
                    Text = "You haven't purchased any tickets yet.\nBrowse events and click 'Buy Ticket' to get started!",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(lblNoTickets);
            }
            else
            {
                this.Controls.Add(dgvTickets);
            }

            this.Controls.Add(headerPanel);
            this.Controls.Add(accentStrip);

            dgvTickets.DataBindingComplete += (s, e) =>
            {
                if (dgvTickets.Columns.Contains("PurchaseID")) dgvTickets.Columns["PurchaseID"].Visible = false;
                if (dgvTickets.Columns.Contains("UserID")) dgvTickets.Columns["UserID"].Visible = false;
                if (dgvTickets.Columns.Contains("EventID")) dgvTickets.Columns["EventID"].Visible = false;
            };
        }
    }

    public class ClientForm : Form
    {
        private DataGridView eventGrid = new DataGridView();
        private EventManagementSystem _system;
        private UserManagementSystem _userSystem;
        private DatabaseManager _database = new DatabaseManager();

        //tracks who is logged in, null means no one is logged in
        private string _loggedInUsername = null;

        //top panel controls for the login/logout bar
        private Panel topPanel = new Panel();
        private Button btnLogin = new Button();
        private Label lblUsername = new Label();
        private Button btnLogout = new Button();

        // Weather panel controls
        private Panel weatherPanel = new Panel();
        private Label lblWeatherIcon = new Label();
        private Label lblWeatherTemp = new Label();
        private Label lblWeatherDesc = new Label();
        private Label lblWeatherTitle = new Label();
        private Button btnRefreshWeather = new Button();

        // Search panel controls
        private Panel searchPanel = new Panel();
        private TextBox txtSearch = new TextBox();
        private ComboBox cmbTypeFilter = new ComboBox();
        private bool _buyHandlerAttached = false;
        private Button btnMyTickets = new Button();


        public ClientForm(EventManagementSystem system, UserManagementSystem userSystem, string loggedInUsername = null)
        {
            _system = system;
            _userSystem = userSystem;
            _loggedInUsername = loggedInUsername;
            this.Text = "Wembley Events - Client Portal";
            this.Size = new Size(1120, 490);

            //sets up the top bar that holds the login and logout controls
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 48;
            topPanel.BackColor = Color.FromArgb(0, 55, 115);

            // Gold accent strip at top of the bar
            Panel topAccent = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = Color.FromArgb(255, 190, 0) };
            topPanel.Controls.Add(topAccent);

            // App title on the left of the bar
            Label lblAppTitle = new Label
            {
                Text = "Wembley Events",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(14, 12),
                Size = new Size(200, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
            topPanel.Controls.Add(lblAppTitle);

            //login button shown when no user is logged in
            btnLogin.Text = "Login";
            btnLogin.Location = new Point(694, 11);
            btnLogin.Size = new Size(88, 26);
            btnLogin.BackColor = Color.FromArgb(255, 190, 0);
            btnLogin.ForeColor = Color.FromArgb(0, 40, 90);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            //username label hidden by default, shows after login
            lblUsername.Location = new Point(390, 15);
            lblUsername.Size = new Size(200, 18);
            lblUsername.ForeColor = Color.FromArgb(195, 220, 255);
            lblUsername.Font = new Font("Segoe UI", 9);
            lblUsername.TextAlign = ContentAlignment.MiddleRight;
            lblUsername.Visible = false;

            //logout button hidden by default, shows after login
            btnLogout.Text = "Logout";
            btnLogout.Location = new Point(694, 11);
            btnLogout.Size = new Size(88, 26);
            btnLogout.BackColor = Color.FromArgb(175, 35, 35);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Visible = false;
            btnLogout.Click += BtnLogout_Click;

            //adds the login/logout controls to the top panel
            topPanel.Controls.Add(btnLogin);
            topPanel.Controls.Add(lblUsername);
            topPanel.Controls.Add(btnLogout);

            //adds the "My Tickets" button to the top panel, visible only when logged in
            btnMyTickets.Text = "My Tickets";
            btnMyTickets.Location = new Point(596, 11);
            btnMyTickets.Size = new Size(92, 26);
            btnMyTickets.BackColor = Color.FromArgb(255, 190, 0);
            btnMyTickets.ForeColor = Color.FromArgb(0, 40, 90);
            btnMyTickets.FlatStyle = FlatStyle.Flat;
            btnMyTickets.FlatAppearance.BorderSize = 0;
            btnMyTickets.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMyTickets.Cursor = Cursors.Hand;
            btnMyTickets.Visible = false;
            btnMyTickets.Click += BtnMyTickets_Click;
            topPanel.Controls.Add(btnMyTickets);

            weatherPanel.Dock = DockStyle.Top;
            weatherPanel.Height = 60;
            weatherPanel.BackColor = Color.FromArgb(0, 40, 90);

            lblWeatherTitle = new Label
            {
                Text = "Wembley Stadium Weather",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 200, 255),
                Location = new Point(15, 5),
                Size = new Size(200, 18)
            };

            lblWeatherIcon = new Label
            {
                Text = "☁",
                Font = new Font("Segoe UI", 20),
                ForeColor = Color.White,
                Location = new Point(15, 22),
                Size = new Size(40, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblWeatherTemp = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(55, 24),
                Size = new Size(120, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblWeatherDesc = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 230),
                Location = new Point(175, 30),
                Size = new Size(300, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnRefreshWeather = new Button
            {
                Text = "↻",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(720, 18),
                Size = new Size(35, 30),
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            //Button to refresh the weather data 
            btnRefreshWeather.FlatAppearance.BorderSize = 0;
            btnRefreshWeather.Click += (s, ev) => LoadWeatherAsync();

            weatherPanel.Controls.Add(lblWeatherTitle);
            weatherPanel.Controls.Add(lblWeatherIcon);
            weatherPanel.Controls.Add(lblWeatherTemp);
            weatherPanel.Controls.Add(lblWeatherDesc);
            weatherPanel.Controls.Add(btnRefreshWeather);

            eventGrid.Dock = DockStyle.Fill;
            eventGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            eventGrid.AllowUserToAddRows = false;

            // Grid styling
            eventGrid.EnableHeadersVisualStyles = false;
            eventGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 55, 115);
            eventGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            eventGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            eventGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            eventGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            eventGrid.ColumnHeadersHeight = 34;
            eventGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 246, 255);
            eventGrid.DefaultCellStyle.Padding = new Padding(6, 3, 6, 3);
            eventGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(195, 215, 255);
            eventGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 30, 50);
            eventGrid.GridColor = Color.FromArgb(220, 228, 245);
            eventGrid.BorderStyle = BorderStyle.None;
            eventGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            eventGrid.BackgroundColor = Color.White;
            eventGrid.RowHeadersVisible = false;

            // Search panel
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 48;
            searchPanel.BackColor = Color.White;

            // Bottom border on the search panel
            Panel searchBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(210, 220, 235) };
            searchPanel.Controls.Add(searchBorder);

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(12, 15),
                Size = new Size(52, 18),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 70, 110)
            };
            txtSearch = new TextBox
            {
                Location = new Point(67, 12),
                Size = new Size(195, 22),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyDown += (s, ev) => { if (ev.KeyCode == Keys.Enter) ApplySearch(); };

            Label lblType = new Label
            {
                Text = "Type:",
                Location = new Point(275, 15),
                Size = new Size(38, 18),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 70, 110)
            };
            cmbTypeFilter = new ComboBox
            {
                Location = new Point(315, 12),
                Size = new Size(112, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbTypeFilter.Items.AddRange(new string[] { "All", "Football", "Concert", "Comedy", "Other" });
            cmbTypeFilter.SelectedIndex = 0;

            Button btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(438, 11),
                Size = new Size(70, 26),
                BackColor = Color.FromArgb(0, 55, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, ev) => ApplySearch();

            Button btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(515, 11),
                Size = new Size(52, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(90, 100, 120),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            btnClear.Click += (s, ev) => { txtSearch.Clear(); cmbTypeFilter.SelectedIndex = 0; ApplySearch(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblType);
            searchPanel.Controls.Add(cmbTypeFilter);
            searchPanel.Controls.Add(btnSearch);
            searchPanel.Controls.Add(btnClear);

            // Stack order: eventGrid (fill) → searchPanel (top) → weatherPanel (top) → topPanel (top)
            this.Controls.Add(eventGrid);
            this.Controls.Add(searchPanel);
            this.Controls.Add(weatherPanel);
            this.Controls.Add(topPanel);


            // AI Chatbox
            var chatBox = new AIChatBox(ConfigurationManager.AppSettings["AIkey"]);
            chatBox.Size = new Size(300, 380);

            // Position it at the bottom right
            chatBox.Location = new Point(this.ClientSize.Width - 320, this.ClientSize.Height - 440);
            chatBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chatBox.BorderStyle = BorderStyle.FixedSingle;
            chatBox.Visible = false;

            // Create the Chat Toggle Button for the bottom right corner
            Button btnToggleChat = new Button
            {
                Text = "💬 AI Chat",
                Size = new Size(90, 40),
                Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(255, 190, 0),
                ForeColor = Color.FromArgb(0, 55, 115),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleChat.FlatAppearance.BorderSize = 0;

            // Click event to dynamically open and close the chat
            btnToggleChat.Click += (s, ev) =>
            {
                chatBox.Visible = !chatBox.Visible;
                if (chatBox.Visible)
                {
                    chatBox.BringToFront(); // Makes sure it pops up over the data grid
                }
            };

            // Add both controls to the form
            this.Controls.Add(chatBox);
            this.Controls.Add(btnToggleChat);

            // Forces them to render on top of all other elements
            chatBox.BringToFront();
            btnToggleChat.BringToFront();

            LoadEvents();

            //applies the correct login/logout UI state on startup
            UpdateLoginUI();

            // Load weather data asynchronously on startup
            LoadWeatherAsync();
        }



        private void BtnLogin_Click(object sender, EventArgs e)
        {
            //opens the login form and closes the current client form
            var loginForm = new LoginUser.LoginForm(_userSystem, _system, this);
            loginForm.Show();

            // Closes the current client form to ensure only one instance is open at a time
            //this.Hide();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            //clears the logged in user and updates the UI back to logged out state
            _loggedInUsername = null;
            UpdateLoginUI();
        }

        public void SetLoggedInUser(string username)
        {
            _loggedInUsername = username;
            UpdateLoginUI();
        }

        public WembleyEvent[] GetEvents()
        {
            return _system.GetAllEvents();
        }

        private void UpdateLoginUI()
        {
            bool loggedIn = _loggedInUsername != null;

            //shows the login button when logged out, hides it when logged in
            btnLogin.Visible = !loggedIn;

            //shows the username and logout button only when someone is logged in
            lblUsername.Text = loggedIn ? $"Welcome, {_loggedInUsername}" : "";
            lblUsername.Visible = loggedIn;
            btnLogout.Visible = loggedIn;

            //refreshes the grid to reflect login state
            eventGrid.Refresh();

            //shows the "My Tickets" button only when logged in
            btnMyTickets.Visible = loggedIn;
        }

        private void BtnMyTickets_Click(object sender, EventArgs e)
        {
            // Find the logged-in user's ID
            UserNode[] users = _userSystem.GetAllUsers();
            User currentUser = null;

            foreach (var userNode in users)
            {
                if (string.Equals(userNode.User.Username, _loggedInUsername, StringComparison.OrdinalIgnoreCase))
                {
                    currentUser = userNode.User;
                    break;
                }
            }

            if (currentUser == null)
            {
                MessageBox.Show("Please log in first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get purchases from DB and show the tickets form
            Purchase[] purchases = _database.GetPurchasesByUser(currentUser.UserID);
            new MyTicketsForm(purchases, _loggedInUsername).ShowDialog();
        }

        private void LoadEvents()
        {
            RefreshGrid(_system.GetAllEvents());

            if (!_buyHandlerAttached)
            {
                // Add Buy button column once
                var buyCol = new DataGridViewButtonColumn()
                {
                    Name = "BuyButton",
                    HeaderText = "Action",
                    Text = "Buy Ticket",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat,
                    Width = 110
                };
                buyCol.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                buyCol.DefaultCellStyle.ForeColor = Color.FromArgb(0, 55, 115);
                buyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                eventGrid.Columns.Add(buyCol);

                // Wire click handler once
                eventGrid.CellContentClick += (s, e) =>
                {
                    if (e.RowIndex < 0 || !eventGrid.Columns.Contains("BuyButton")) return;
                    if (e.ColumnIndex != eventGrid.Columns["BuyButton"].Index) return;

                    //if not logged in, open login form first
                    if (_loggedInUsername == null)
                    {
                        var loginForm = new LoginUser.LoginForm(_userSystem, _system, this);
                        if (loginForm.ShowDialog(this) == DialogResult.OK)
                        {
                            _loggedInUsername = loginForm.loggedinUser.Username;
                            UpdateLoginUI();
                        }
                        else
                        {
                            return; //login cancelled, skip purchase
                        }
                    }

                    // DataBoundItem holds the full WembleyEvent object regardless of column visibility
                    // Hidden columns (EventID, BusinessID) do not affect data access via DataBoundItem
                    var selectedEvent = eventGrid.Rows[e.RowIndex].DataBoundItem as WembleyEvent;
                    if (selectedEvent == null)
                    {
                        Console.WriteLine("Error: could not retrieve event data from selected row");
                        return;
                    }

                    Console.WriteLine($"Buy clicked - EventID accessed from hidden column: {selectedEvent.EventID}");

                    //increase the attendance for the event by 1
                    selectedEvent.Attendance += 1;

                    //updates the attendance for the event
                    _system.UpdateEvent(selectedEvent.EventID, selectedEvent);

                    UserNode[] users = _userSystem.GetAllUsers();
                    User purchaseUser = null;
                    //finds the user object for the currently logged in user to get their user id for the purchase record
                    foreach (var userNode in users)
                    {
                        if (string.Equals(userNode.User.Username, _loggedInUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            purchaseUser = userNode.User;
                            break;
                        }
                    }
                    //saves the purchase to the database with the user id and event id
                    if (purchaseUser != null)
                    {
                       _database.InsertPurchase(purchaseUser.UserID, selectedEvent.EventID);
                    }

                    //opens the UI to show the message
                    new PurchaseConfirmationForm().ShowDialog();


                    ApplySearch(); // Re-apply search to refresh the grid with updated attendance
                    eventGrid.Refresh();

                };

                _buyHandlerAttached = true;
            }
        }

        private void RefreshGrid(WembleyEvent[] events)
        {
            eventGrid.DataSource = null;
            eventGrid.DataSource = events;

            //This will prevent the button column placed first after searching
            if (eventGrid.Columns.Contains("BuyButton"))
                eventGrid.Columns.Remove("BuyButton");

            // Hide internal ID columns
            if (eventGrid.Columns.Contains("EventID"))
                eventGrid.Columns["EventID"].Visible = false;
            if (eventGrid.Columns.Contains("BusinessID"))
                eventGrid.Columns["BusinessID"].Visible = false;


            // Re-add Buy button column if cleared by DataSource change
            if (_buyHandlerAttached && !eventGrid.Columns.Contains("BuyButton"))
            {
                var buyCol = new DataGridViewButtonColumn()
                {
                    Name = "BuyButton",
                    HeaderText = "Action",
                    Text = "Buy Ticket",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat,
                    Width = 110
                };
                buyCol.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                buyCol.DefaultCellStyle.ForeColor = Color.FromArgb(0, 55, 115);
                buyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                eventGrid.Columns.Add(buyCol);
            }

        }

        //Refreshes the event grid whenever the form becomes visible again
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                ApplySearch(); // This refreshes the grid with current data from the tree
            }
        }

        private void ApplySearch()
        {
            string name = txtSearch.Text.Trim().ToLower();
            string type = cmbTypeFilter.SelectedItem?.ToString() ?? "All";

            WembleyEvent[] all = _system.GetAllEvents();

            int count = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                bool nameOk = string.IsNullOrEmpty(name) || all[i].EventName.ToLower().Contains(name);
                bool typeOk = type == "All" || all[i].EventType == type;
                if (nameOk && typeOk) count++;
            }

            WembleyEvent[] filtered = new WembleyEvent[count];
            int idx = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                bool nameOk = string.IsNullOrEmpty(name) || all[i].EventName.ToLower().Contains(name);
                bool typeOk = type == "All" || all[i].EventType == type;
                if (nameOk && typeOk) filtered[idx++] = all[i];
            }

            RefreshGrid(filtered);
        }

        private async void LoadWeatherAsync()
        {

            //Show loading while is calling the api and disable the refresh button to prevent multiple calls
            lblWeatherTemp.Text = "Loading...";
            lblWeatherDesc.Text = "";
            btnRefreshWeather.Enabled = false;

            //Async call to get the weather data
            WeatherResult result = await WeatherService.GetCurrentWeatherAsync();


            //Update the weather panel with the result
            if (result.Success)
            {
                lblWeatherIcon.Text = GetWeatherEmoji(result.IconCode);
                lblWeatherTemp.Text = $"{result.Temperature}°C";
                lblWeatherDesc.Text = result.Description;
            }
            //Error message in case the api call is failed
            else
            {
                lblWeatherIcon.Text = "⚠";
                lblWeatherTemp.Text = "Unavailable";
                lblWeatherDesc.Text = "Could not load weather data";
            }

            btnRefreshWeather.Enabled = true;
        }

        private string GetWeatherEmoji(string iconCode)
        {
            //Maps OpenWeatherMap icon codes to simple weather emojis
            if (string.IsNullOrEmpty(iconCode)) return "☁";

            string baseCode = iconCode.Length >= 2 ? iconCode.Substring(0, 2) : iconCode;

            switch (baseCode)
            {
                case "01": return "☀";
                case "02": return "⛅";
                case "03": return "☁";
                case "04": return "☁";
                case "09": return "🌧";
                case "10": return "🌦";
                case "11": return "⛈";
                case "13": return "❄";
                case "50": return "🌫";
                default: return "☁";
            }
        }
    }

    //Database Part
    public class DatabaseManager
    {
        string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        //Wembley Events
        public void LoadEventsIntoTree(EventBinaryTree tree)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT EventID, EventName, EventDate, EventType, Attendance, EventPrice, BusinessID FROM Events ORDER BY EventID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WembleyEvent wembleyEvent = new WembleyEvent(
                                reader.GetInt32(0), // EventID
                                reader.GetInt32(6), // BusinessID
                                reader.GetString(1), // EventName
                                reader.GetDateTime(2), // EventDate
                                reader.GetString(3), // EventType
                                reader.GetInt32(4), // Attendance
                                reader.GetInt32(5)  // EventPrice
                            );

                            //Insert the event into the binary tree
                            tree.Insert(wembleyEvent);
                        }
                    }
                }
            }
        }

        public void InsertEvent(WembleyEvent wembleyEvent)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {

                string sql = @"INSERT INTO Events
                           (EventID, BusinessID, EventName, EventDate, EventType, Attendance, EventPrice)
                           VALUES
                           (@EventID, @BusinessID, @EventName, @EventDate, @EventType, @Attendance, @EventPrice)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", wembleyEvent.EventID);
                    cmd.Parameters.AddWithValue("@BusinessID", wembleyEvent.BusinessID); // ADDED
                    cmd.Parameters.AddWithValue("@EventName", wembleyEvent.EventName);
                    cmd.Parameters.AddWithValue("@EventDate", wembleyEvent.EventDate);
                    cmd.Parameters.AddWithValue("@EventType", wembleyEvent.EventType);
                    cmd.Parameters.AddWithValue("@Attendance", wembleyEvent.Attendance);
                    cmd.Parameters.AddWithValue("@EventPrice", wembleyEvent.EventPrice);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateEvent(WembleyEvent wembleyEvent)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {

                string sql = @"UPDATE Events
                           SET BusinessID = @BusinessID,
                               EventName = @EventName,
                               EventDate = @EventDate,
                               EventType = @EventType,
                               Attendance = @Attendance,
                               EventPrice = @EventPrice
                           WHERE EventID = @EventID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", wembleyEvent.EventID);
                    cmd.Parameters.AddWithValue("@BusinessID", wembleyEvent.BusinessID); // ADDED
                    cmd.Parameters.AddWithValue("@EventName", wembleyEvent.EventName);
                    cmd.Parameters.AddWithValue("@EventDate", wembleyEvent.EventDate);
                    cmd.Parameters.AddWithValue("@EventType", wembleyEvent.EventType);
                    cmd.Parameters.AddWithValue("@Attendance", wembleyEvent.Attendance);
                    cmd.Parameters.AddWithValue("@EventPrice", wembleyEvent.EventPrice);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteEvent(int eventId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "DELETE FROM Events WHERE EventID = @EventID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Users
        public void LoadUsersIntoList(UserLinkedList userList)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT UserID, Username, Email, Password, UserRole FROM Users ORDER BY UserID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                UserRole = reader.GetString(4)
                            };

                            userList.AddUser(user);
                        }
                    }
                }
            }
        }

        public void InsertUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"INSERT INTO Users (UserID, Username, Email, Password, UserRole)
                           VALUES (@UserID, @Username, @Email, @Password, @UserRole)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@UserRole", user.UserRole);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"UPDATE Users
                           SET Username = @Username,
                               Email = @Email,
                               Password = @Password,
                               UserRole = @UserRole
                           WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@UserRole", user.UserRole);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "DELETE FROM Users WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Insert Purchases on the database when user bought a ticket
        public void InsertPurchase(int userId, int eventId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"INSERT INTO Purchases (UserID, EventID, PurchaseDate)
                           VALUES (@UserID, @EventID, @PurchaseDate)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Get the purchase history of the user by joining the Purchases and Events tables to get the event details for each purchase
        public Purchase[] GetPurchasesByUser(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = @"SELECT p.PurchaseID, p.UserID, p.EventID,e.EventName, e.EventDate, e.EventType, e.EventPrice, p.PurchaseDate
                               FROM Purchases p
                               INNER  JOIN Events e ON p.EventID = e.EventID
                               WHERE p.UserID = @UserID
                               ORDER BY p.PurchaseDate DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        //Using a list to store the purchases temporarily while reading from the database
                        var tempList = new System.Collections.Generic.List<Purchase>();
                        while (reader.Read())
                        {
                            tempList.Add(new Purchase()
                            {
                                PurchaseID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                EventID = reader.GetInt32(2),
                                EventName = reader.GetString(3),
                                EventDate = reader.GetDateTime(4),
                                EventType = reader.GetString(5),
                                EventPrice = reader.GetInt32(6),
                                PurchaseDate = reader.GetDateTime(7)
                            });
                        }
                        return tempList.ToArray();
                    }
                }
            }
        }
    }

        

        //Wembley Event Part
        public class EventNode
        {
            public WembleyEvent Event { get; set; }
            public EventNode Right { get; set; } // Pointer to the right child node (events with greater EventID)
            public EventNode Left { get; set; } // Pointer to the left child node (events with smaller EventID)

            public EventNode(WembleyEvent wembleyEvent)
            {
                Event = wembleyEvent;
            }

            public int GetEventID()
            {
                if (Event != null)
                {
                    return Event.EventID;
                }

                return -1; // Returning error
            }
        }

        public class EventBinaryTree
        {
            private EventNode root;
            public int currentMaxEventID = 0; // To keep track of the current maximum event ID for assigning new IDs

            public EventBinaryTree()
            {
                root = null;
            }

            //Get root node
            public EventNode GetRoot()
            {
                return root;
            }

            //Inserting
            public void Insert(WembleyEvent wembleyEvent)
            {
                if (root == null)
                {
                    root = new EventNode(wembleyEvent);
                    currentMaxEventID = wembleyEvent.EventID;
                    return;
                }

                if (wembleyEvent.EventID > currentMaxEventID)
                {
                    currentMaxEventID = wembleyEvent.EventID;
                }


                InsertRec(root, wembleyEvent);

                //Rebalancing the tree after every inserting new node
                Rebalance();
            }

            private void InsertRec(EventNode currentRoot, WembleyEvent wembleyEvent)
            {
                if (currentRoot.GetEventID() < wembleyEvent.EventID)
                {
                    if (currentRoot.Right == null)
                    {
                        currentRoot.Right = new EventNode(wembleyEvent);
                    }
                    else
                    {
                        InsertRec(currentRoot.Right, wembleyEvent);
                    }
                }
                else if (currentRoot.GetEventID() > wembleyEvent.EventID)
                {
                    if (currentRoot.Left == null)
                    {
                        currentRoot.Left = new EventNode(wembleyEvent);
                    }
                    else
                    {
                        InsertRec(currentRoot.Left, wembleyEvent);
                    }
                }

            }

            //Delete
            public void Delete(WembleyEvent wembleyEvent)
            {
                root = DeleteRec(root, wembleyEvent.EventID);
            }

            private EventNode DeleteRec(EventNode currentRoot, int eventID)
            {
                // Base case: empty tree
                if (currentRoot == null)
                {
                    return null;
                }

                //Traverse the tree to find the node to delete
                if (eventID < currentRoot.GetEventID())
                {
                    currentRoot.Left = DeleteRec(currentRoot.Left, eventID);
                }
                else if (eventID > currentRoot.GetEventID())
                {
                    currentRoot.Right = DeleteRec(currentRoot.Right, eventID);
                }
                else
                {
                    //Node with no children
                    if (currentRoot.Left == null && currentRoot.Right == null)
                    {
                        return null;
                    }

                    //Node with only one child
                    if (currentRoot.Left == null)
                    {
                        return currentRoot.Right;
                    }
                    if (currentRoot.Right == null)
                    {
                        return currentRoot.Left;
                    }

                    //Node with two children
                    EventNode successor = FindMin(currentRoot.Right);
                    currentRoot.Event = successor.Event;
                    currentRoot.Right = DeleteRec(currentRoot.Right, successor.GetEventID());
                }

                return currentRoot;
            }

            private EventNode FindMin(EventNode node)
            {
                //Find the leftmost node in the subtree
                while (node.Left != null)
                {
                    node = node.Left;
                }
                return node;
            }

            //Searching
            public EventNode FindEvent(int eventID)
            {
                return FindEventRec(root, eventID);
            }

            public EventNode FindEventRec(EventNode currentRoot, int eventID)
            {
                if (currentRoot.GetEventID() < eventID)
                {
                    if (currentRoot.Right != null)
                    {
                        if (currentRoot.Right.GetEventID() == eventID)
                        {
                            return currentRoot.Right;
                        }
                        else
                        {
                            return FindEventRec(currentRoot.Right, eventID);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (currentRoot.GetEventID() > eventID)
                {
                    if (currentRoot.Left != null)
                    {
                        if (currentRoot.Left.GetEventID() == eventID)
                        {
                            return currentRoot.Left;
                        }
                        else
                        {
                            return FindEventRec(currentRoot.Left, eventID);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return currentRoot;
                }

            }

            //Rebalancing Tree
            private void Rebalance()
            {

                if (root == null)
                {
                    return;
                }

                //Get all events in sorted order
                int size = GetSize();
                WembleyEvent[] events = new WembleyEvent[size];
                int index = 0;
                GetAll(root, events, ref index);

                //Rebuild tree from sorted array
                root = BuildBalancedTree(events, 0, size - 1);
            }

            private EventNode BuildBalancedTree(WembleyEvent[] events, int start, int end)
            {
                //TODO maybe some improvement

                if (start > end)
                {
                    return null;
                }

                //Making the middle element root
                int mid = start + (end - start) / 2;
                EventNode node = new EventNode(events[mid]);

                node.Left = BuildBalancedTree(events, start, mid - 1);
                node.Right = BuildBalancedTree(events, mid + 1, end);

                return node;
            }

            //Get size of tree
            public int GetSize()
            {
                return GetSizeRec(root);
            }

            private int GetSizeRec(EventNode node)
            {
                if (node == null)
                {
                    return 0;
                }

                return 1 + GetSizeRec(node.Left) + GetSizeRec(node.Right);
            }

            //Get all events in array
            public void GetAll(EventNode node, WembleyEvent[] events, ref int index)
            {
                if (node == null)
                {
                    return;
                }

                // In-order traversal (left, current, right)
                GetAll(node.Left, events, ref index);
                events[index] = node.Event;
                index++;
                GetAll(node.Right, events, ref index);
            }

            //Printing Tree
            public void Print()
            {
                PrintRec(root, "", false);
            }

            private void PrintRec(EventNode node, string indent, bool isLeft)
            {
                if (node == null)
                {
                    Console.WriteLine(indent + (isLeft ? "└── " : "┌── ") + "∅");
                    return;
                }

                // Print right subtree
                if (node.Right != null)
                {
                    PrintRec(node.Right, indent + (isLeft ? "    " : "│   "), false);
                }
                else
                {
                    Console.WriteLine(indent + (isLeft ? "    " : "│   ") + "┌── ∅");
                }

                // Print current node
                Console.WriteLine(indent + (isLeft ? "└── " : "┌── ") + $"{node.GetEventID()}");

                // Print left subtree
                if (node.Left != null)
                {
                    PrintRec(node.Left, indent + (isLeft ? "│   " : "    "), true);
                }
                else
                {
                    Console.WriteLine(indent + (isLeft ? "│   " : "    ") + "└── ∅");
                }
            }

        }

        public class WembleyEvent
        {
            public int EventID { get; set; } // Unique identifier for the event
            public int BusinessID { get; set; } // Tracks who owns the event
            public string EventName { get; set; } // Name of the event (Tottnham vs Arsenal)
            public DateTime EventDate { get; set; } // Date and time of the event
            public string EventType { get; set; } // Type of event (Football, Concert)
            public int Attendance { get; set; } // Number of attendees expected or recorded for the event
            public int EventPrice { get; set; } // Price of the event ticket in GBP

            public WembleyEvent(int EventID, int businessID, string eventName, DateTime eventDate, string eventType, int attendance, int eventPrice)
            {
                this.EventID = EventID;
                this.BusinessID = businessID;
                this.EventName = eventName;
                this.EventDate = eventDate;
                this.EventType = eventType;
                this.Attendance = attendance;
                this.EventPrice = eventPrice;
            }
        }

        public class EventManagementSystem
        {
            //TODO: public for test should be private
            public EventBinaryTree tree;
            private DatabaseManager database;

            public EventManagementSystem()
            {
                tree = new EventBinaryTree();
                database = new DatabaseManager();

                LoadEventsFromDatabase();
            }

            public void LoadEventsFromDatabase()
            {
                database.LoadEventsIntoTree(tree);
            }

            public void AddEvent(WembleyEvent wembleyEvent)
            {
                wembleyEvent.EventID = tree.currentMaxEventID + 1; // Assign a new unique ID to the event

                tree.Insert(wembleyEvent);
                database.InsertEvent(wembleyEvent);
            }

            public void UpdateEvent(int eventId, WembleyEvent updatedEvent)
            {
                EventNode node = tree.FindEvent(eventId);
                if (node != null)
                {
                    node.Event.EventName = updatedEvent.EventName;
                    node.Event.EventDate = updatedEvent.EventDate;
                    node.Event.EventType = updatedEvent.EventType;
                    node.Event.Attendance = updatedEvent.Attendance;
                    node.Event.EventPrice = updatedEvent.EventPrice;

                    database.UpdateEvent(node.Event);
                }
            }

            public void DeleteEvent(int eventId)
            {
                EventNode node = tree.FindEvent(eventId);
                if (node != null)
                {
                    tree.Delete(node.Event);
                    database.DeleteEvent(eventId);
                }
            }

            public WembleyEvent GetEvent(int eventId)
            {
                EventNode node = tree.FindEvent(eventId);
                return node != null ? node.Event : null;
            }

            public WembleyEvent[] GetAllEvents()
            {
                int size = tree.GetSize();
                WembleyEvent[] events = new WembleyEvent[size];
                int index = 0;
                tree.GetAll(tree.GetRoot(), events, ref index);
                return events;
            }
        }

        //User Part
        public class UserNode
        {
            public User User { get; set; }
            public UserNode Next { get; set; } //pointer to the next node in the linked list
            public UserNode(User user)
            {
                User = user;
                Next = null;
            }
        }

        public class UserLinkedList
        {
            private UserNode root;
            private int currentMaxUserID = 0; // To keep track of the current maximum user ID for assigning new IDs

            public UserLinkedList()
            {
                root = null;
            }

            public void AddUser(User user)
            {
                UserNode newNode = new UserNode(user);

                if (root == null)
                {
                    root = newNode;
                    currentMaxUserID = user.UserID;
                }
                else
                {
                    user.UserID = currentMaxUserID + 1; // Assign a new unique ID to the user
                    currentMaxUserID = user.UserID;

                    //traverse to the end of the list and add the new node
                    UserNode current = root;
                    while (current.Next != null)
                    {
                        current = current.Next;
                    }

                    current.Next = newNode;
                }
            }

            public void UpdateUser(int userId, User updatedUser)
            {
                //traverse the list to find the user and update the information
                UserNode current = root;
                while (current != null)
                {
                    if (current.User.UserID == userId)
                    {
                        current.User = updatedUser;
                        return;
                    }

                    current = current.Next;
                }
            }

            public void DeleteUser(int userId)
            {
                if (root == null) return;

                //if the root node is the one to delete get the next node and make it the new root
                if (root.User.UserID == userId)
                {
                    root = root.Next;
                    return;
                }

                //traverse the list to find the user and delete it by changing the next pointer of the previous node to skip the deleted node
                UserNode current = root;
                while (current.Next != null)
                {
                    if (current.Next.User.UserID == userId)
                    {
                        current.Next = current.Next.Next;
                        return;
                    }
                    current = current.Next;
                }
            }

            public User GetUser(int userId)
            {
                UserNode current = root;
                while (current != null)
                {
                    if (current.User.UserID == userId)
                    {
                        return current.User;
                    }
                    current = current.Next;
                }
                return null; // Not found
            }

            public UserNode[] GetAllUsers()
            {
                //traverse the list to count the number of users and store them in an array
                int size = 0;
                UserNode current = root;
                while (current != null)
                {
                    size++;
                    current = current.Next;
                }

                //create an array of the correct size and fill it with the users
                UserNode[] users = new UserNode[size];
                current = root;
                int index = 0;
                while (current != null)
                {
                    users[index++] = current;
                    current = current.Next;
                }

                return users;
            }

        }

        public class User
        {
            public int UserID { get; set; } // Unique identifier for the user
            public string Username { get; set; } // Name of the user
            public string Email { get; set; } // Email address of the user
            public string Password { get; set; } // Password for user authentication
            public string UserRole { get; set; } // Admin, Client, Unverified_Business, Verified_Business

            public User()
            {

            }
        }
        //Purchase class to store the purchase information when a user buys a ticket for an event
        public class Purchase
        {
            public int PurchaseID { get; set; } // Unique identifier for the purchase
            public int UserID { get; set; } // ID of the user who made the purchase
            public int EventID { get; set; } // ID of the event that was purchased
            public string EventName { get; set; } // Name of the event that was purchased
            public DateTime PurchaseDate { get; set; } // Date and time of the purchase
            public DateTime EventDate { get; set; } // Date and time of the event
            public int EventPrice { get; set; } // Price of the event at the time of purchase
            public string EventType { get; set; } // Type of the event at the time of purchase

        }

        public class UserManagementSystem
        {
            private UserLinkedList userList;
            private DatabaseManager database;

            public UserManagementSystem()
            {
                userList = new UserLinkedList();
                database = new DatabaseManager();

                LoadFromDatabase();
            }

            public void LoadFromDatabase()
            {
                database.LoadUsersIntoList(userList);
            }

            public void RegisterUser(User user)
            {
                userList.AddUser(user);
                database.InsertUser(user);
            }

            public void UpdateUser(int userId, User updatedUser)
            {
                userList.UpdateUser(userId, updatedUser);
                database.UpdateUser(updatedUser);
            }

            public void DeleteUser(int userId)
            {
                userList.DeleteUser(userId);
                database.DeleteUser(userId);
            }

            public User GetUser(int userId)
            {
                return userList.GetUser(userId);
            }

            public UserNode[] GetAllUsers()
            {
                return userList.GetAllUsers();
            }

            public void PrintAllUsers()
            {
                UserNode[] users = userList.GetAllUsers();
                foreach (var userNode in users)
                {
                    Console.WriteLine($"UserID: {userNode.User.UserID}, Username: {userNode.User.Username}, Email: {userNode.User.Email}, Role: {userNode.User.UserRole}");
                }
            }

            //Method to match username and password
            //Returns null if no match is found

            public User FindByCredentials(string username, string password)
            {
                UserNode[] users = userList.GetAllUsers();
                //Check every user in the linked list to find a match
                foreach (var node in users)
                {
                    if (string.Equals(node.User.Username, username, StringComparison.OrdinalIgnoreCase) && node.User.Password == password)
                    {
                        return node.User;
                    }
                }
                //No match found
                return null;
            }

            //Method to check if the username already exists
            //Return true if it exists, false if does not exist

            public bool UsernameExists(string username)
            {

                UserNode[] users = userList.GetAllUsers();

                foreach (var node in users)
                {

                    if (string.Equals(node.User.Username, username, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        //Main Program
        public static class Program
        {

            public static void Main()
            {
                //Wembley Event Binary tree test

                /*
                EventBinaryTree eventBinaryTree = new EventBinaryTree();

                for (int i = 0; i < 10; ++i)
                {
                    eventBinaryTree.Insert(new WembleyEvent(i));
                }

                eventBinaryTree.Print();

                EventNode test = eventBinaryTree.FindEvent(8);

                Console.WriteLine("Found Node is " + test.GetEventID().ToString());

                Console.WriteLine("-------------------");

                eventBinaryTree.Delete(eventBinaryTree.FindEvent(5).Event);

                eventBinaryTree.Print();

                //User Linked List test
                UserManagementSystem userManagementSystem = new UserManagementSystem();

                for (int i = 0; i < 10; ++i)
                {
                    userManagementSystem.RegisterUser(new User() { UserID = i, Username = "User" + i });
                }

                userManagementSystem.PrintAllUsers();

                userManagementSystem.UpdateUser(5, new User() { UserID = 5, Username = "UpdatedUser5" });

                userManagementSystem.DeleteUser(3);

                Console.WriteLine("-------------------");

                Console.WriteLine("Get User with ID 5: " + userManagementSystem.GetUser(5)?.Username);

                Console.ReadLine();

                userManagementSystem.PrintAllUsers();
                */

                //Load events from database
                EventManagementSystem eventManagementSystem = new EventManagementSystem();

                //TEST DO NOT UNCOMMENT except you want to test the db functions
                //EventID is auto generated so we can just set it to 0 when creating a new event and the system will assign a new unique ID to it

                //WembleyEvent newEvent = new WembleyEvent(0,"Test Event", DateTime.Now, "Football", 0, 50);
                //eventManagementSystem.AddEvent(newEvent);

                //eventManagementSystem.DeleteEvent(101);

                //eventManagementSystem.UpdateEvent(103, new WembleyEvent(0,"Updated Test Event", DateTime.Now, "Football", 0, 50));

                eventManagementSystem.tree.Print();

                UserManagementSystem userManagementSystem = new UserManagementSystem();

                User newUser = new User()
                {
                    Username = "TestUser",
                    Email = "test@gmail.com",
                    Password = "password123",
                    UserRole = "Client"
                };

                //userManagementSystem.RegisterUser(newUser);

                newUser.Username = "UpdatedTestUser";
                //userManagementSystem.UpdateUser(2, newUser);

                //userManagementSystem.DeleteUser(5);

                //UI
                //Changed after reduce our .NET version
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //Login Form
                //Application.Run(new LoginUser.LoginForm(userManagementSystem, eventManagementSystem));

                //Register Form
                //Application.Run(new RegisterUser.RegisterFormClient(userManagementSystem));

                //Business Register Form
                //Application.Run(new RegisterFormBusiness(userManagementSystem));

                //Client Form
                Application.Run(new ClientForm(eventManagementSystem, userManagementSystem));

                //Admin Form
                //Application.Run(new AdminUser.AdminBusinessForm(eventManagementSystem, userManagementSystem, new User() { UserRole = "Admin" }));
            }
        }
    }
