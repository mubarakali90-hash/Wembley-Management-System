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
            this.Size = new Size(850, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Buttons
            btnAddEvent = new Button { Text = "Add New Event", Location = new Point(20, 20), Size = new Size(130, 40), BackColor = Color.LightGreen, Cursor = Cursors.Hand };
            btnAddEvent.Click += BtnAddEvent_Click;

            btnUpdateEvent = new Button { Text = "Update Selected", Location = new Point(160, 20), Size = new Size(130, 40), BackColor = Color.LightYellow, Cursor = Cursors.Hand };
            btnUpdateEvent.Click += BtnUpdateEvent_Click;

            btnDeleteEvent = new Button { Text = "Delete Selected", Location = new Point(300, 20), Size = new Size(130, 40), BackColor = Color.LightCoral, Cursor = Cursors.Hand };
            btnDeleteEvent.Click += BtnDeleteEvent_Click;

            // Admin Only Button
            btnManageUsers = new Button { Text = "Manage Users", Location = new Point(680, 20), Size = new Size(130, 40), BackColor = Color.LightBlue, Cursor = Cursors.Hand };
            btnManageUsers.Click += BtnManageUsers_Click;
            btnManageUsers.Visible = _currentUser.UserRole == "Admin"; // Hide if Business

            // DataGridView for Events
            dgvEvents = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(790, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            // Add controls
            this.Controls.Add(btnAddEvent);
            this.Controls.Add(btnUpdateEvent);
            this.Controls.Add(btnDeleteEvent);
            this.Controls.Add(btnManageUsers);
            this.Controls.Add(dgvEvents);
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
            this.Size = new Size(350, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblName = new Label { Text = "Event Name:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtEventName = new TextBox { Location = new Point(130, 18), Size = new Size(180, 25) };

            Label lblDate = new Label { Text = "Date:", Location = new Point(20, 60), Size = new Size(100, 20) };
            dtpEventDate = new DateTimePicker { Location = new Point(130, 58), Size = new Size(180, 25) };

            Label lblType = new Label { Text = "Type:", Location = new Point(20, 100), Size = new Size(100, 20) };
            cmbEventType = new ComboBox { Location = new Point(130, 98), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });

            Label lblPrice = new Label { Text = "Price (£):", Location = new Point(20, 140), Size = new Size(100, 20) };
            numPrice = new NumericUpDown { Location = new Point(130, 138), Size = new Size(180, 25), Maximum = 1000 };

            Button btnSave = new Button { Text = "Save Event", Location = new Point(50, 200), Size = new Size(100, 35), BackColor = Color.LightGreen };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(180, 200), Size = new Size(100, 35), BackColor = Color.LightGray };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
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
            this.Size = new Size(350, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblName = new Label { Text = "Event Name:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtEventName = new TextBox { Location = new Point(130, 18), Size = new Size(180, 25), Text = _eventToUpdate.EventName };

            Label lblDate = new Label { Text = "Date:", Location = new Point(20, 60), Size = new Size(100, 20) };
            dtpEventDate = new DateTimePicker { Location = new Point(130, 58), Size = new Size(180, 25), Value = _eventToUpdate.EventDate };

            Label lblType = new Label { Text = "Type:", Location = new Point(20, 100), Size = new Size(100, 20) };
            cmbEventType = new ComboBox { Location = new Point(130, 98), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });
            cmbEventType.SelectedItem = _eventToUpdate.EventType;

            Label lblPrice = new Label { Text = "Price (£):", Location = new Point(20, 140), Size = new Size(100, 20) };
            numPrice = new NumericUpDown { Location = new Point(130, 138), Size = new Size(180, 25), Maximum = 1000, Value = _eventToUpdate.EventPrice };

            Button btnSave = new Button { Text = "Save Changes", Location = new Point(50, 200), Size = new Size(110, 35), BackColor = Color.LightYellow };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(180, 200), Size = new Size(100, 35), BackColor = Color.LightGray };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
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