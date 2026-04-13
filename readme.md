# Wembley Management System

A C# Windows Forms desktop application for managing Wembley events, users, and ticket purchases.

## Project Overview

The Wembley Management System allows:
- **Clients** to register, log in, browse events, search and sort events, buy tickets, and view their ticket history.
- **Business users** to register for a business account and, once verified, manage their own events.
- **Admins** to manage all events and verify or update business user roles.
- **Users/Admins** to interact with an AI chatbox for event-related questions.

The project was built for the **CST2550 Software Engineering Management and Development** group coursework.

## Main Features

- Custom **Binary Search Tree (BST)** for storing and searching events by `EventID`
- Custom **Linked List** for storing users
- Custom **Merge Sort** for sorting events in the UI
- SQL database integration for:
  - `Users`
  - `Events`
  - `Purchases`
- Windows Forms user interface
- Login and registration system
- Ticket purchase flow with attendance updates
- Admin and business dashboard
- Weather display for Wembley Stadium
- AI chatbox integration

## Technologies Used

- C#
- Windows Forms
- SQL Server
- MSTest
- .NET / Visual Studio

## Project Structure

Typical repository contents:

- `WembleyMain.cs` – main program, data structures, database code, and client UI
- `LoginForm.cs` – login form
- `RegisterFormClient.cs` – client account registration
- `RegisterFormBusiness.cs` – business account registration
- `AdminBusinessForm.cs` – admin/business dashboard
- `ManageUsersForm.cs` – manage business user roles
- `AIChatbox.cs` – AI chat assistant
- `DataStructureAndAlgorithmTests.cs` – MSTest unit tests
- `SQLCreationScript.txt` / `SQLCreationScript_fixed.txt` – SQL setup script
- Project report PDF
- Demo video
- Meeting minutes / project management files

## Configuration

The application expects a SQL Server connection string named **`DefaultConnection`**.

Create an `App.config` file inside the WembleyManagementSystem folder:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="DefaultConnection"
         connectionString="" />
  </connectionStrings>

  <appSettings>
    <add key="MySetting" value="HelloWorld"/>
  </appSettings>
</configuration>
```

#### Database Setup

We already added your school mail to our Azure so you should be able to open these links.

1. Click this link https://portal.azure.com/?l=en.en-gb#@livemdxac.onmicrosoft.com/resource/subscriptions/18caacdd-31f0-4c62-9e0d-1d37eba60723/resourceGroups/Wembley/providers/Microsoft.Sql/servers/wembley/networking.
2. On the **Firewall Rules** section click **Add your client IPV4 address (Your ip will show up here)**
3. Then click this link https://portal.azure.com/?l=en.en-gb#@livemdxac.onmicrosoft.com/resource/subscriptions/18caacdd-31f0-4c62-9e0d-1d37eba60723/resourceGroups/Wembley/providers/Microsoft.Sql/servers/wembley/databases/free-sql-db-3371518/connectionStrings
4. Copy the all **ADO.NET (SQL authentication)** connection string and paste it to `App.config` **connectionString=** section.
5. As **{your_password}** write mEWBW!hwAKM*G8FBsEokQAK
5. Now you should be able to connect the database when you run the app.

## How to Build and Run

1. Open the project/solution in **Visual Studio**.
2. Make sure the database has been created and the connection string is correct.
3. Make sure any required `App.config` settings are present.
4. Run the project.

The application currently starts from the **Client Form**.

## How to Use the Application

### 1. Client user
- Launch the application.
- Browse the list of events.
- Use the search box and event type filter.
- Sort by clicking the event grid column headers.
- Click **Login** to sign in.
- If you do not have an account, register as a client.
- After login, click **Buy Ticket** for an event.
- Use **My Tickets** to view previous purchases.

### 2. Business user
- Register for a business account.
- New business accounts are created as **Unverified_Business**.
- Once verified by an admin, the business user can access the dashboard.
- Verified business users can add, edit, and delete their own events.

### 3. Admin user
- Log in with an admin account.
- Open the admin dashboard.
- Add, edit, or delete events.
- Open **Manage Users** to change business account roles.
- Admins can view all events, while business users only see their own events.

### 4. AI chatbox
- Open the AI chat panel from the client or admin/business UI.
- Ask event-related questions.
- The chatbox uses the current event data from the BST.
- In the admin/business dashboard, the AI assistant can also help execute event-related actions.

## User Roles

The system uses these roles:
- `Client`
- `Unverified_Business`
- `Verified_Business`
- `Admin`

## Running the Tests

Unit tests are included using **MSTest**.

To run tests in Visual Studio:
1. Open **Test Explorer**
2. Build the solution
3. Run all tests

The tests cover:
- BST insertion, search, and deletion
- Linked list user operations
- Merge sort ordering

## Known Limitations

- Passwords are stored in plain text and are **not secure** for real-world use.
- The tree balancing approach rebuilds the BST instead of using a self-balancing tree.
- The linked list is slower for searching large numbers of users.
- API-dependent features require valid configuration.
- The database schema must match the current insert logic for IDs.

## License

This project was created for academic coursework.
