using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

namespace WembleyManagementSystem
{
    public class DatabaseManager
    {
class Program {
  static void Main() {
    var password = "Wembley123.";
    var cs = $"Server=wembley-management-db.cnsmqce46enu.eu-west-2.rds.amazonaws.com,1433;Database=<wembley-management-sql>;User ID=admin;Password=Wembley123.;Encrypt=True;TrustServerCertificate=True;";
    SqlConnection conn = null;
    try {
      conn = new SqlConnection(cs);
      conn.Open();
      using var cmd = new SqlCommand("SELECT @@VERSION", conn);
      Console.WriteLine(cmd.ExecuteScalar());
    } catch (Exception ex) {
      Console.WriteLine($"Database error: {ex.Message}");
      throw;
    } finally {
      conn?.Close();
      conn?.Dispose();
    }
  }
}
        }
public void AddEvent(string name, DateTime date, string location)
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string query = "INSERT INTO Events (EventName, EventDate, Location) VALUES (@name,@date,@location)";

        SqlCommand cmd = new SqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@date", date);
        cmd.Parameters.AddWithValue("@location", location);

        cmd.ExecuteNonQuery();
    }
}

public void DeleteEvent(int eventId)
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string query = "DELETE FROM Events WHERE EventId=@id";

        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", eventId);

        cmd.ExecuteNonQuery();
    }
}

public void AddUser(string name, string email, string role)
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string query = "INSERT INTO Users (Name, Email, Role) VALUES (@name,@email,@role)";

        SqlCommand cmd = new SqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@role", role);

        cmd.ExecuteNonQuery();
    }
}

public void DeleteUser(int userId)
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string query = "DELETE FROM Users WHERE UserId=@id";

        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", userId);

        cmd.ExecuteNonQuery();
    }
}

public void GetUsers()
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string query = "SELECT * FROM Users";

        SqlCommand cmd = new SqlCommand(query, conn);

        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine(reader["Name"] + " - " + reader["Role"]);
        }
    }
}


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
    }

    public class ClientForm : Form
    {
        private DataGridView eventGrid = new DataGridView();
        private EventManagementSystem _system;
       
        
        public ClientForm(EventManagementSystem system)
{
    _system = system;

    this.Text = "Wembley Events - Client Portal";
    this.Size = new Size(800, 450);

    eventGrid.Dock = DockStyle.Fill;
    eventGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    eventGrid.AllowUserToAddRows = false;

    this.Controls.Add(eventGrid);

    Button chatBtn = new Button()
    {
        Text = "💬 AI Assistant",
        Dock = DockStyle.Bottom,
        Height = 40,
        BackColor = Color.FromArgb(90, 60, 180),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Cursor = Cursors.Hand
    };

    chatBtn.FlatAppearance.BorderSize = 0;
    chatBtn.Click += (s, e) => new AIChatForm().ShowDialog();

    this.Controls.Add(chatBtn);

    // Load data
    LoadEvents();
}

        private void LoadEvents()
        {
            //Shows all the event using the getAllEvents function
            eventGrid.DataSource = null;
            eventGrid.DataSource = _system.GetAllEvents();

            // adds the buy button
            if (!eventGrid.Columns.Contains("BuyButton"))
            {
                var buyCol = new DataGridViewButtonColumn()
                {
                    Name = "BuyButton",
                    HeaderText = "Action",
                    Text = "Buy",
                    UseColumnTextForButtonValue = true
                };
                eventGrid.Columns.Add(buyCol);
            }

            eventGrid.CellContentClick += (s, e) =>
            {
                if (e.ColumnIndex == eventGrid.Columns["BuyButton"].Index && e.RowIndex >= 0)
                {
                    var selectedEvent = (WembleyEvent)eventGrid.Rows[e.RowIndex].DataBoundItem;

                    //incres the attendance for the event by 1
                    selectedEvent.Attendance += 1;

                    //updates the attendance for the event
                    _system.UpdateEvent(selectedEvent.EventID, selectedEvent);

                    //opens tge UI to show the message
                    new PurchaseConfirmationForm().ShowDialog();

                    eventGrid.Refresh();
                }
            };
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
                return;
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
                    return new EventNode(new WembleyEvent(-1));
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
                    return new EventNode(new WembleyEvent(-1));
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

        public WembleyEvent(int EventID /* just for test need to add all later */)
        {
            this.EventID = EventID;
        }
    }

    public class EventManagementSystem
    {
        private EventBinaryTree tree;

        public EventManagementSystem()
        {
            tree = new EventBinaryTree();
        }

        public void AddEvent(WembleyEvent wembleyEvent)
        {
            tree.Insert(wembleyEvent);
        }

        public void UpdateEvent(int eventId, WembleyEvent updatedEvent)
        {
            var node = tree.FindEvent(eventId);
            if (node != null) node.Event.Attendance = updatedEvent.Attendance;
        }

        public void DeleteEvent(int eventId)
        {
            tree.Delete(tree.FindEvent(eventId).Event);
        }

        public WembleyEvent GetEvent(int eventId)
        {
            return tree.FindEvent(eventId).Event;
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
            }
            else
            {
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
    }

    public class UserManagementSystem
    {
        private UserLinkedList userList;

        public UserManagementSystem()
        {
            userList = new UserLinkedList();
        }

        public void RegisterUser(User user)
        {
            userList.AddUser(user);
        }

        public void UpdateUser(int userId, User updatedUser)
        {
            userList.UpdateUser(userId, updatedUser);
        }

        public void DeleteUser(int userId)
        {
            userList.DeleteUser(userId);
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
    }

    //Main Program
    public static class Program
    {

        public static void Main()
        {
            //Wembley Event Binary tree test

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

            userManagementSystem.PrintAllUsers();

            //UI
            ApplicationConfiguration.Initialize();

            EventManagementSystem system = new EventManagementSystem();
            system.AddEvent(new WembleyEvent(101) { EventName = "Tottenham vs Arsenal", Attendance = 50000 });
            system.AddEvent(new WembleyEvent(102) { EventName = "Coldplay Concert", Attendance = 80000 });

            Application.Run(new ClientForm(system));

        }
    }
}