using System;

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
        EventNode currentRoot = FindEvent(wembleyEvent.EventID);
    }

    public void DeleteRec(EventNode currentRoot, WembleyEvent wembleyEvent)
    {
        //TODO
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

    //Rebulding Tree
    private void Rebalance()
    {
        if (root != null)
        {
            //TODO
        }
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

public class EventManagmentSystem
{
    public void AddEvent(WembleyEvent wembleyEvent)
    {

    }

    public void UpdateEvent(int eventId, WembleyEvent updatedEvent)
    {

    }

    public void DeleteEvent(int eventId)
    {

    }

    public WembleyEvent GetEvent(int eventId)
    {
        return null;
    }

    public List<WembleyEvent> GetAllEvents()
    {
        return new List<WembleyEvent>();
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

public class UserMnagementSystem
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

public class Program
{
    static void Main(string[] args)
    {
        EventBinaryTree eventBinaryTree = new EventBinaryTree();

        for (int i = 0; i < 10; ++i)
        {
            eventBinaryTree.Insert(new WembleyEvent(i));
        }

        eventBinaryTree.Print();

        EventNode test = eventBinaryTree.FindEvent(8);

        Console.WriteLine("Found Node is " + test.GetEventID().ToString());
    }
}
