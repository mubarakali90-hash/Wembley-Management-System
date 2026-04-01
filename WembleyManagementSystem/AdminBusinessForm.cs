using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace user
{
    public class AdminBusinessForm : Form
    {
        private EventManagementSystem _eventSystem;
        private UserManagementSystem _userSystem;
        private User _currentUser;

        private DataGridView dgvEvents;
        private TextBox txtEventName;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private NumericUpDown numPrice;
        private Button btnAddEvent;
        private Button btnDeleteEvent;
        private Button btnManageUsers;
        private Button btnUpdateEvent;

        public AdminBusinessForm(EventManagementSystem eventSystem, UserManagementSystem userSystem, User currentUser)
        {
            _eventSystem = eventSystem;
            _userSystem = userSystem;
            _currentUser = currentUser;
            InitializeComponents();
            LoadEvents();
        }

        private void InitializeComponents()
        {
            this.Text = _currentUser.UserRole == "Admin" ? "Admin Dashboard" : "Business Dashboard";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Input Fields
            Label lblName = new Label { Text = "Event Name:", Location = new Point(20, 20), Size = new Size(80, 20) };
            txtEventName = new TextBox { Location = new Point(100, 20), Size = new Size(150, 25) };

            Label lblDate = new Label { Text = "Date:", Location = new Point(270, 20), Size = new Size(40, 20) };
            dtpEventDate = new DateTimePicker { Location = new Point(320, 20), Size = new Size(150, 25) };

            Label lblType = new Label { Text = "Type:", Location = new Point(20, 60), Size = new Size(80, 20) };
            cmbEventType = new ComboBox { Location = new Point(100, 60), Size = new Size(150, 25) };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });

            Label lblPrice = new Label { Text = "Price (£):", Location = new Point(270, 60), Size = new Size(60, 20) };
            numPrice = new NumericUpDown { Location = new Point(330, 60), Size = new Size(140, 25), Maximum = 1000 };

            // Buttons
            btnAddEvent = new Button { Text = "Add Event", Location = new Point(500, 20), Size = new Size(100, 30), BackColor = Color.LightGreen };
            btnAddEvent.Click += BtnAddEvent_Click;

            btnDeleteEvent = new Button { Text = "Delete Selected", Location = new Point(500, 60), Size = new Size(100, 30), BackColor = Color.LightCoral };
            btnDeleteEvent.Click += BtnDeleteEvent_Click;

            // NEW: Update Event Button
            btnUpdateEvent = new Button { Text = "Update Selected", Location = new Point(610, 60), Size = new Size(130, 30), BackColor = Color.LightYellow };
            btnUpdateEvent.Click += BtnUpdateEvent_Click;

            // Admin Only Button
            btnManageUsers = new Button { Text = "Manage Users", Location = new Point(620, 20), Size = new Size(120, 35), BackColor = Color.LightBlue };
            btnManageUsers.Click += BtnManageUsers_Click;
            btnManageUsers.Visible = _currentUser.UserRole == "Admin"; // Hide if Business

            // DataGridView for Events
            dgvEvents = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(740, 420),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            
            // Auto-fill inputs when selecting an event to edit
            dgvEvents.SelectionChanged += DgvEvents_SelectionChanged;

            // Add controls
            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
            this.Controls.Add(btnAddEvent); this.Controls.Add(btnDeleteEvent);
            this.Controls.Add(btnUpdateEvent); // Add new button to form
            this.Controls.Add(btnManageUsers); this.Controls.Add(dgvEvents);
        }
        
        private void LoadEvents()
        {
            var allEvents = _eventSystem.GetAllEvents();
            
            // Filter logic based on role
            if (_currentUser.UserRole == "Verified_Business")
            {
                // Filters the array so businesses ONLY see their own events
                dgvEvents.DataSource = allEvents.Where(e => e != null && e.BusinessID == _currentUser.UserID).ToArray();
            }
            else // Admin
            {
                // Admins see everything
                dgvEvents.DataSource = allEvents.Where(e => e != null).ToArray();
            }
        }

        // Auto-fills the text boxes when you click an event in the grid
        private void DgvEvents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;
                txtEventName.Text = selectedEvent.EventName;
                dtpEventDate.Value = selectedEvent.EventDate;
                cmbEventType.SelectedItem = selectedEvent.EventType;
                numPrice.Value = selectedEvent.EventPrice;
            }
        }
        
        private void BtnAddEvent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            WembleyEvent newEvent = new WembleyEvent(
                0, // EventID (auto-generates)
                _currentUser.UserID, // BusinessID (The owner)
                txtEventName.Text,
                dtpEventDate.Value,
                cmbEventType.SelectedItem.ToString(),
                0, // Initial attendance
                (int)numPrice.Value
            );

            newEvent.BusinessID = _currentUser.UserID;

            _eventSystem.AddEvent(newEvent);
            LoadEvents(); // Refresh Grid
            MessageBox.Show("Event Added Successfully!");
        }

        // Handles the actual updating logic
        private void BtnUpdateEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
                {
                    MessageBox.Show("Please fill all fields to update.");
                    return;
                }

                var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;
                
                selectedEvent.EventName = txtEventName.Text;
                selectedEvent.EventDate = dtpEventDate.Value;
                selectedEvent.EventType = cmbEventType.SelectedItem.ToString();
                selectedEvent.EventPrice = (int)numPrice.Value;

                _eventSystem.UpdateEvent(selectedEvent.EventID, selectedEvent);
                LoadEvents(); // Refresh Grid
                MessageBox.Show("Event Updated Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select an event from the list to update.");
            }
        }
        
        private void BtnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                // Ask for confirmation before deleting
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
        }

        private void BtnManageUsers_Click(object sender, EventArgs e)
        {
            ManageUsersForm usersForm = new ManageUsersForm(_userSystem);
            usersForm.ShowDialog();
        }
    }
}