using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace AdminUser
{
    public class AdminBusinessForm : Form
    {
        private EventManagementSystem _eventSystem;
        private UserManagementSystem _userSystem;
        private User _currentUser;

        private DataGridView dgvEvents;
        private Button btnAddEvent;
        private Button btnUpdateEvent;
        private Button btnDeleteEvent;
        private Button btnManageUsers;
        private readonly Form _lastOpenedForm;

        public AdminBusinessForm(EventManagementSystem eventSystem, UserManagementSystem userSystem, User currentUser, Form lastOpenedForm = null)
        {
            _eventSystem = eventSystem;
            _userSystem = userSystem;
            _lastOpenedForm = lastOpenedForm;
            _currentUser = currentUser;
            InitializeComponents();
            LoadEvents();
        }

        override protected void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (_lastOpenedForm != null)
            {
                _lastOpenedForm.Show();
            }
        }

        private void InitializeComponents()
        {
            this.Text = _currentUser.UserRole == "Admin" ? "Admin Dashboard" : "Business Dashboard";
            this.Size = new Size(870, 590);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 252);

            // Gold accent strip at top
            Panel accentStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Color.FromArgb(255, 190, 0)
            };

            // Header panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(0, 55, 115)
            };
            string roleLabel = _currentUser.UserRole == "Admin" ? "Administrator" : "Business";
            Label lblUserInfo = new Label
            {
                Text = $"  {_currentUser.Username}   \u2022   {roleLabel}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(195, 220, 255),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label lblDashTitle = new Label
            {
                Text = _currentUser.UserRole == "Admin" ? "Admin Dashboard  " : "Business Dashboard  ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Right,
                Width = 200,
                TextAlign = ContentAlignment.MiddleRight
            };
            headerPanel.Controls.Add(lblUserInfo);
            headerPanel.Controls.Add(lblDashTitle);

            // Button toolbar panel
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            Panel toolbarBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(210, 220, 235)
            };
            toolbarPanel.Controls.Add(toolbarBorder);

            // Add button
            btnAddEvent = new Button
            {
                Text = "+ Add Event",
                Location = new Point(15, 12),
                Size = new Size(115, 34),
                BackColor = Color.FromArgb(34, 120, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddEvent.FlatAppearance.BorderSize = 0;
            btnAddEvent.Click += BtnAddEvent_Click;

            // Update button
            btnUpdateEvent = new Button
            {
                Text = "Edit Selected",
                Location = new Point(140, 12),
                Size = new Size(115, 34),
                BackColor = Color.FromArgb(160, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateEvent.FlatAppearance.BorderSize = 0;
            btnUpdateEvent.Click += BtnUpdateEvent_Click;

            // Delete button
            btnDeleteEvent = new Button
            {
                Text = "Delete Selected",
                Location = new Point(265, 12),
                Size = new Size(115, 34),
                BackColor = Color.FromArgb(175, 35, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteEvent.FlatAppearance.BorderSize = 0;
            btnDeleteEvent.Click += BtnDeleteEvent_Click;

            // Manage Users (Admin only)
            btnManageUsers = new Button
            {
                Text = "Manage Users",
                Location = new Point(710, 12),
                Size = new Size(120, 34),
                BackColor = Color.FromArgb(0, 55, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = _currentUser.UserRole == "Admin"
            };
            btnManageUsers.FlatAppearance.BorderSize = 0;
            btnManageUsers.Click += BtnManageUsers_Click;

            toolbarPanel.Controls.Add(btnAddEvent);
            toolbarPanel.Controls.Add(btnUpdateEvent);
            toolbarPanel.Controls.Add(btnDeleteEvent);
            toolbarPanel.Controls.Add(btnManageUsers);

            // DataGridView
            dgvEvents = new DataGridView
            {
                Location = new Point(15, 130),
                Size = new Size(828, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(225, 230, 240)
            };

            this.Controls.Add(dgvEvents);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(accentStrip);
        }

        private void LoadEvents()
        {
            var allEvents = _eventSystem.GetAllEvents();

            // Filter logic based on role
            if (_currentUser.UserRole == "Verified_Business")
            {
                dgvEvents.DataSource = allEvents.Where(e => e != null && e.BusinessID == _currentUser.UserID).ToArray();
            }
            else // Admin
            {
                dgvEvents.DataSource = allEvents.Where(e => e != null).ToArray();
            }

            if (dgvEvents.Columns.Contains("EventID")) dgvEvents.Columns["EventID"].Visible = false;
            if (dgvEvents.Columns.Contains("BusinessID")) dgvEvents.Columns["BusinessID"].Visible = false;
        }

        private void BtnAddEvent_Click(object sender, EventArgs e)
        {
            // Open the Add Form. If they click Save, refresh the grid
            AddEventForm addForm = new AddEventForm(_eventSystem, _currentUser);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadEvents();
            }
        }

        private void BtnUpdateEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;

                // Open the new Update Form and pass the selected event.
                UpdateEventForm updateForm = new UpdateEventForm(_eventSystem, selectedEvent);
                if (updateForm.ShowDialog() == DialogResult.OK)
                {
                    LoadEvents();
                }
            }
            else
            {
                MessageBox.Show("Please select an event from the list to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this event? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;
                    _eventSystem.DeleteEvent(selectedEvent.EventID);
                    LoadEvents(); // Refresh Grid
                }
            }
            else
            {
                MessageBox.Show("Please select an event from the list to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnManageUsers_Click(object sender, EventArgs e)
        {
            ManageUser.ManageUsersForm usersForm = new ManageUser.ManageUsersForm(_userSystem);
            usersForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AdminBusinessForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "AdminBusinessForm";
            this.Load += new System.EventHandler(this.AdminBusinessForm_Load);
            this.ResumeLayout(false);

        }

        private void AdminBusinessForm_Load(object sender, EventArgs e)
        {

        }
    }

    // Form for adding events
    public class AddEventForm : Form
    {
        private TextBox txtEventName;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private NumericUpDown numPrice;
        private EventManagementSystem _eventSystem;
        private User _currentUser;

        public AddEventForm(EventManagementSystem eventSystem, User currentUser)
        {
            _eventSystem = eventSystem;
            _currentUser = currentUser;

            this.Text = "Add New Event";
            this.Size = new Size(380, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 252);

            // Gold accent strip
            Panel accentStrip = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = Color.FromArgb(255, 190, 0) };

            // Header
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(0, 55, 115) };
            Label lblHeader = new Label
            {
                Text = "Add New Event",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };
            headerPanel.Controls.Add(lblHeader);

            // Event Name
            Label lblName = new Label { Text = "Event Name", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 65), Size = new Size(310, 16) };
            txtEventName = new TextBox { Location = new Point(25, 83), Size = new Size(310, 24), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };

            // Date and Type side by side
            Label lblDate = new Label { Text = "Date", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 122), Size = new Size(145, 16) };
            dtpEventDate = new DateTimePicker { Location = new Point(25, 140), Size = new Size(148, 24), Font = new Font("Segoe UI", 9) };

            Label lblType = new Label { Text = "Type", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(187, 122), Size = new Size(145, 16) };
            cmbEventType = new ComboBox { Location = new Point(187, 140), Size = new Size(148, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });

            // Price
            Label lblPrice = new Label { Text = "Price (£)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 182), Size = new Size(145, 16) };
            numPrice = new NumericUpDown { Location = new Point(25, 200), Size = new Size(148, 24), Maximum = 1000, Font = new Font("Segoe UI", 9) };

            // Separator
            Panel sep = new Panel { Location = new Point(25, 246), Size = new Size(310, 1), BackColor = Color.FromArgb(210, 220, 235) };

            // Buttons
            Button btnSave = new Button
            {
                Text = "Save Event",
                Location = new Point(25, 256),
                Size = new Size(145, 34),
                BackColor = Color.FromArgb(34, 120, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(190, 256),
                Size = new Size(145, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 95, 120),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(accentStrip);
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
            this.Controls.Add(sep);
            this.Controls.Add(btnSave); this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            WembleyEvent newEvent = new WembleyEvent(
                0,
                _currentUser.UserID,
                txtEventName.Text,
                dtpEventDate.Value,
                cmbEventType.SelectedItem.ToString(),
                0,
                (int)numPrice.Value
            );

            _eventSystem.AddEvent(newEvent);
            MessageBox.Show("Event Added Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK; // Tells the main form to refresh the grid
            this.Close();
        }
    }

    // Form for updating events
    public class UpdateEventForm : Form
    {
        private TextBox txtEventName;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private NumericUpDown numPrice;
        private EventManagementSystem _eventSystem;
        private WembleyEvent _eventToUpdate;

        public UpdateEventForm(EventManagementSystem eventSystem, WembleyEvent eventToUpdate)
        {
            _eventSystem = eventSystem;
            _eventToUpdate = eventToUpdate;

            this.Text = "Update Event";
            this.Size = new Size(380, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 252);

            // Gold accent strip
            Panel accentStrip = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = Color.FromArgb(255, 190, 0) };

            // Header
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(0, 55, 115) };
            Label lblHeader = new Label
            {
                Text = "Edit Event",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };
            headerPanel.Controls.Add(lblHeader);

            // Event Name
            Label lblName = new Label { Text = "Event Name", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 65), Size = new Size(310, 16) };
            txtEventName = new TextBox { Location = new Point(25, 83), Size = new Size(310, 24), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Text = _eventToUpdate.EventName };

            // Date and Type side by side
            Label lblDate = new Label { Text = "Date", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 122), Size = new Size(145, 16) };
            dtpEventDate = new DateTimePicker { Location = new Point(25, 140), Size = new Size(148, 24), Font = new Font("Segoe UI", 9), Value = _eventToUpdate.EventDate };

            Label lblType = new Label { Text = "Type", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(187, 122), Size = new Size(145, 16) };
            cmbEventType = new ComboBox { Location = new Point(187, 140), Size = new Size(148, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });
            cmbEventType.SelectedItem = _eventToUpdate.EventType;

            // Price
            Label lblPrice = new Label { Text = "Price (£)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 90), Location = new Point(25, 182), Size = new Size(145, 16) };
            numPrice = new NumericUpDown { Location = new Point(25, 200), Size = new Size(148, 24), Maximum = 1000, Font = new Font("Segoe UI", 9), Value = _eventToUpdate.EventPrice };

            // Separator
            Panel sep = new Panel { Location = new Point(25, 246), Size = new Size(310, 1), BackColor = Color.FromArgb(210, 220, 235) };

            // Buttons
            Button btnSave = new Button
            {
                Text = "Save Changes",
                Location = new Point(25, 256),
                Size = new Size(145, 34),
                BackColor = Color.FromArgb(160, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(190, 256),
                Size = new Size(145, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 95, 120),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(accentStrip);
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
            this.Controls.Add(sep);
            this.Controls.Add(btnSave); this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _eventToUpdate.EventName = txtEventName.Text;
            _eventToUpdate.EventDate = dtpEventDate.Value;
            _eventToUpdate.EventType = cmbEventType.SelectedItem.ToString();
            _eventToUpdate.EventPrice = (int)numPrice.Value;

            _eventSystem.UpdateEvent(_eventToUpdate.EventID, _eventToUpdate);
            MessageBox.Show("Event Updated Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK; // Tells the main form to refresh the grid
            this.Close();
        }
    }
}