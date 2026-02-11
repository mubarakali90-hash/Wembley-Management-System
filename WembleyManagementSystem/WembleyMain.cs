using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace WembleyManagementSystem
{
    //My code
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
        private EventManagmentSystem _system;

        public ClientForm(EventManagmentSystem system)
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

            eventGrid.CellContentClick += (s, e) => {
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

    public class WembleyEvent
    {
        public int EventID { get; set; } // Unique identifier for the event
        public string EventName { get; set; } // Name of the event (Tottnham vs Arsenal)
        public DateTime EventDate { get; set; } // Date and time of the event
        public string EventType { get; set; } // Type of event (Football, Concert)
        public int Attendance { get; set; } // Number of attendees expected or recorded for the event
        public int EventPrice { get; set; } // Price of the event ticket in GBP

        public WembleyEvent(int EventID)
        {
            this.EventID = EventID;
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

        public void Insert(WembleyEvent wembleyEvent)
        {
            if (root == null)
            {
                root = new EventNode(wembleyEvent);
                return;
            }
            InsertRec(root, wembleyEvent);
        }

        private void InsertRec(EventNode currentRoot, WembleyEvent wembleyEvent)
        {
            if (currentRoot.GetEventID() < wembleyEvent.EventID)
            {
                if (currentRoot.Right == null) currentRoot.Right = new EventNode(wembleyEvent);
                else InsertRec(currentRoot.Right, wembleyEvent);
            }
            else if (currentRoot.GetEventID() > wembleyEvent.EventID)
            {
                if (currentRoot.Left == null) currentRoot.Left = new EventNode(wembleyEvent);
                else InsertRec(currentRoot.Left, wembleyEvent);
            }
        }

        public void GetAll(EventNode node, List<WembleyEvent> list)
        {
            if (node == null) return;
            GetAll(node.Left, list);
            list.Add(node.Event);
            GetAll(node.Right, list);
        }

        public EventNode FindEvent(int eventID) => FindEventRec(root, eventID);

        private EventNode FindEventRec(EventNode currentRoot, int eventID)
        {
            if (currentRoot == null || currentRoot.GetEventID() == eventID) return currentRoot;
            return eventID < currentRoot.GetEventID() ? FindEventRec(currentRoot.Left, eventID) : FindEventRec(currentRoot.Right, eventID);
        }

        public EventNode GetRoot() => root;
    }

    public class EventManagmentSystem
    {
        private EventBinaryTree _tree = new EventBinaryTree();

        public void AddEvent(WembleyEvent wembleyEvent) => _tree.Insert(wembleyEvent);

        public List<WembleyEvent> GetAllEvents()
        {
            List<WembleyEvent> events = new List<WembleyEvent>();
            _tree.GetAll(_tree.GetRoot(), events);
            return events;
        }

        public void UpdateEvent(int eventId, WembleyEvent updatedEvent)
        {
            var node = _tree.FindEvent(eventId);
            if (node != null) node.Event.Attendance = updatedEvent.Attendance;
        }
    }

    // runs the winform app!
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            EventManagmentSystem system = new EventManagmentSystem();
            system.AddEvent(new WembleyEvent(101) { EventName = "Tottenham vs Arsenal", Attendance = 50000 });
            system.AddEvent(new WembleyEvent(102) { EventName = "Coldplay Concert", Attendance = 80000 });

            Application.Run(new ClientForm(system));
        }
    }
}