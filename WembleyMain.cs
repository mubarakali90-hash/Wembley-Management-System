using System;

public class WembleyEvent
{
    public int EventID { get; set; } // Unique identifier for the event
    public string EventName { get; set; } // Name of the event (Tottnham vs Arsenal)
    public DateTime EventDate { get; set; } // Date and time of the event
    public string EventType { get; set; } // Type of event (Football, Concert)
    public int Attendance { get; set; } // Number of attendees expected or recorded for the event
    public int EventPrice { get; set; } // Price of the event ticket in GBP
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

public class Class1
{
    public Class1()
    {

    }
}
