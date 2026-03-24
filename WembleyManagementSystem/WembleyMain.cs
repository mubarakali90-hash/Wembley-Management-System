using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using user;

namespace WembleyManagementSystem
{
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

    public class ClientForm : Form
    {
        private DataGridView eventGrid = new DataGridView();
        private EventManagementSystem _system;
        private User _loggedInUser;

        //tracks who is logged in, null means no one is logged in
        private string _loggedInUsername = null;

        //top panel controls for the login/logout bar
        private Panel topPanel = new Panel();
        private Button btnLogin = new Button();
        private Label lblUsername = new Label();
        private Button btnLogout = new Button();

        public ClientForm(EventManagementSystem system, User loggedinUser)
        {
            _system = system;
            _loggedInUser = loggedinUser;
            this.Text = "Wembley Events - Client Portal";
            this.Size = new Size(800, 490);

            //sets up the top bar that holds the login and logout controls
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 40;
            topPanel.BackColor = Color.FromArgb(30, 30, 60);

            //login button shown when no user is logged in
            btnLogin.Text = "Login";
            btnLogin.Location = new Point(696, 7);
            btnLogin.Size = new Size(88, 26);
            btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            //username label hidden by default, shows after login
            lblUsername.Location = new Point(555, 11);
            lblUsername.Size = new Size(135, 18);
            lblUsername.ForeColor = Color.White;
            lblUsername.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUsername.TextAlign = ContentAlignment.MiddleRight;
            lblUsername.Visible = false;

            //logout button hidden by default, shows after login
            btnLogout.Text = "Logout";
            btnLogout.Location = new Point(696, 7);
            btnLogout.Size = new Size(88, 26);
            btnLogout.BackColor = Color.FromArgb(180, 40, 40);
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

            eventGrid.Dock = DockStyle.Fill;
            eventGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            eventGrid.AllowUserToAddRows = false;

            //adds the grid first so the top panel sits on top of it
            this.Controls.Add(eventGrid);
            this.Controls.Add(topPanel);

            LoadEvents();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            //opens the login form and waits for the result
            var loginForm = new user.LoginForm();
            if (loginForm.ShowDialog(this) == DialogResult.OK)
            {
                //saves the logged in username and updates the UI
                _loggedInUsername = loginForm.LoggedInUsername;
                UpdateLoginUI();
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            //clears the logged in user and updates the UI back to logged out state
            _loggedInUsername = null;
            UpdateLoginUI();
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

            //shows the buy column only when a user is logged in
            if (eventGrid.Columns.Contains("BuyButton"))
                eventGrid.Columns["BuyButton"].Visible = loggedIn;
        }

        private void LoadEvents()
        {
            //Shows all the event using the getAllEvents function
            eventGrid.DataSource = null;
            eventGrid.DataSource = _system.GetAllEvents();

            //adds the buy button hidden by default, only visible after login
            if (!eventGrid.Columns.Contains("BuyButton"))
            {
                var buyCol = new DataGridViewButtonColumn()
                {
                    Name = "BuyButton",
                    HeaderText = "Action",
                    Text = "Buy",
                    UseColumnTextForButtonValue = true,
                    Visible = false
                };
                eventGrid.Columns.Add(buyCol);
            }

            eventGrid.CellContentClick += (s, e) =>
            {
                if (e.ColumnIndex == eventGrid.Columns["BuyButton"].Index && e.RowIndex >= 0)
                {
                    var selectedEvent = (WembleyEvent)eventGrid.Rows[e.RowIndex].DataBoundItem;

                    //increase the attendance for the event by 1
                    selectedEvent.Attendance += 1;

                    //updates the attendance for the event
                    _system.UpdateEvent(selectedEvent.EventID, selectedEvent);

                    //opens the UI to show the message
                    new PurchaseConfirmationForm().ShowDialog();

                    eventGrid.Refresh();
                }
            };
        }
    }

    //Database Part
    public class DatabaseManager
    {
        private readonly string connString = "Server=tcp:wembley.database.windows.net,1433;Initial Catalog=free-sql-db-3371518;Persist Security Info=False;User ID=CloudSA5eb133b3;Password=mEWBW!hwAKM*G8FBsEokQAK;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        //Wembley Events
        public void LoadEventsIntoTree(EventBinaryTree tree)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sql = "SELECT EventID, EventName, EventDate, EventType, Attendance, EventPrice FROM Events ORDER BY EventID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WembleyEvent wembleyEvent = new WembleyEvent(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetString(3), reader.GetInt32(4), reader.GetInt32(5));

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
                           (EventID, EventName, EventDate, EventType, Attendance, EventPrice)
                           VALUES
                           (@EventID, @EventName, @EventDate, @EventType, @Attendance, @EventPrice)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", wembleyEvent.EventID);
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
                           SET EventName = @EventName,
                               EventDate = @EventDate,
                               EventType = @EventType,
                               Attendance = @Attendance,
                               EventPrice = @EventPrice
                           WHERE EventID = @EventID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventID", wembleyEvent.EventID);
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
            else
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
        public string EventName { get; set; } // Name of the event (Tottnham vs Arsenal)
        public DateTime EventDate { get; set; } // Date and time of the event
        public string EventType { get; set; } // Type of event (Football, Concert)
        public int Attendance { get; set; } // Number of attendees expected or recorded for the event
        public int EventPrice { get; set; } // Price of the event ticket in GBP

        public WembleyEvent(int EventID, string eventName, DateTime eventDate, string eventType, int attendance, int eventPrice)
        {
            this.EventID = EventID;
            this.EventName = eventName;
            this.EventDate = eventDate;
            this.EventType = eventType;
            this.Attendance = attendance;
            this.EventPrice = eventPrice;
        }
    }

    public class EventManagementSystem
    {
        //public for test should be private
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
        public string UserRole { get; set; } // Admin, Client, Bussiness

        public User()
        {

        }
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

            //eventManagementSystem.DeleteEvent(104);

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

            userManagementSystem.DeleteUser(5);

            //UI
            //Changed after reduce our .NET version
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EventManagementSystem system = new EventManagementSystem();

            Application.Run(new LoginForm(userManagementSystem, system));
        }
    }
}