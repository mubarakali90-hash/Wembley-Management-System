using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace WembleyManagementSystem
{
    //This is the Ui that shows after someone buys a ticket
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

    //This is the main interface that shows the event list and also the buy button 
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
        private EventBinaryTree _tree;

        public EventManagementSystem()
        {
            _tree = new EventBinaryTree();
        }

        public void AddEvent(WembleyEvent wembleyEvent)
        {
            _tree.Insert(wembleyEvent);
        }

        public void UpdateEvent(int eventId, WembleyEvent updatedEvent)
        {
            var node = _tree.FindEvent(eventId);
            if (node != null) node.Event.Attendance = updatedEvent.Attendance;
        }

        public void DeleteEvent(int eventId)
        {
            _tree.Delete(_tree.FindEvent(eventId).Event);
        }

        public WembleyEvent GetEvent(int eventId)
        {
            return _tree.FindEvent(eventId).Event;
        }

        public WembleyEvent[] GetAllEvents()
        {
            int size = _tree.GetSize();
            WembleyEvent[] events = new WembleyEvent[size];
            int index = 0;
            _tree.GetAll(_tree.GetRoot(), events, ref index);
            return events;
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
        public void RegisterUser(User user)
        {

        }

        public void UpdateUser(int userId, User updatedUser)
        {

        }

        public void DeleteUser(int userId)
        {

        }

        public User GetUser(int userId)
        {
            return null;
        }

        public List<User> GetAllUsers()
        {
            return new List<User>();
        }
    }

    public static class Program
    {

        public static void Main()
        {
            //Binary tree test

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



            //UI
            ApplicationConfiguration.Initialize();

            EventManagementSystem system = new EventManagementSystem();
            system.AddEvent(new WembleyEvent(101) { EventName = "Tottenham vs Arsenal", Attendance = 50000 });
            system.AddEvent(new WembleyEvent(102) { EventName = "Coldplay Concert", Attendance = 80000 });

            Application.Run(new ClientForm(system));

        }
    }
}