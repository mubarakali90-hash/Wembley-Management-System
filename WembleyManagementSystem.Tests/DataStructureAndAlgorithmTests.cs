using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WembleyManagementSystem;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace WembleyManagementSystem.Tests
{
    [TestClass]
    public class DataStructureAndAlgorithmTests
    {
        // 1. EventBinaryTree Tests

        [TestMethod]
        public void EventBinaryTree_Insert_ShouldIncreaseSize()
        {
            // Arrange
            EventBinaryTree tree = new EventBinaryTree();
            WembleyEvent evt1 = new WembleyEvent(1, 1, "Football Match", DateTime.Now, "Football", 0, 100);
            WembleyEvent evt2 = new WembleyEvent(2, 1, "Concert", DateTime.Now, "Concert", 0, 150);

            // Act
            tree.Insert(evt1);
            tree.Insert(evt2);

            // Assert
            Assert.AreEqual(2, tree.GetSize(), "Tree size should be 2 after inserting two events.");
        }

        [TestMethod]
        public void EventBinaryTree_FindEvent_ShouldReturnCorrectEvent()
        {
            // Arrange
            EventBinaryTree tree = new EventBinaryTree();
            WembleyEvent evt = new WembleyEvent(10, 1, "Comedy Show", DateTime.Now, "Comedy", 0, 50);
            tree.Insert(evt);

            // Act
            EventNode foundNode = tree.FindEvent(10);

            // Assert
            Assert.IsNotNull(foundNode, "Node should not be null.");
            Assert.AreEqual("Comedy Show", foundNode.Event.EventName, "Event name should match the inserted event.");
        }

        [TestMethod]
        public void EventBinaryTree_Delete_ShouldRemoveEvent()
        {
            // Arrange
            EventBinaryTree tree = new EventBinaryTree();
            WembleyEvent evt = new WembleyEvent(5, 1, "Gala", DateTime.Now, "Other", 0, 200);
            tree.Insert(evt);

            // Act
            tree.Delete(evt);
            EventNode foundNode = tree.FindEvent(5);

            // Assert
            Assert.IsNull(foundNode, "Node should be null after deletion.");
            Assert.AreEqual(0, tree.GetSize(), "Tree size should be 0 after deleting the only event.");
        }

        // 2. UserLinkedList Tests

        [TestMethod]
        public void UserLinkedList_AddUser_ShouldAssignCorrectID()
        {
            // Arrange
            UserLinkedList list = new UserLinkedList();
            User user1 = new User { Username = "Alice", Email = "alice@test.com" };

            // Act
            list.AddUser(user1);
            User retrievedUser = list.GetUser(user1.UserID);

            // Assert
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("Alice", retrievedUser.Username);
        }

        [TestMethod]
        public void UserLinkedList_DeleteUser_ShouldRemoveFromList()
        {
            // Arrange
            UserLinkedList list = new UserLinkedList();
            User user1 = new User { UserID = 1, Username = "Bob" };
            User user2 = new User { UserID = 2, Username = "Charlie" };
            list.AddUser(user1);
            list.AddUser(user2);

            // Act
            list.DeleteUser(1); // Delete Bob
            User retrievedBob = list.GetUser(1);
            User retrievedCharlie = list.GetUser(2);

            // Assert
            Assert.IsNull(retrievedBob, "Bob should be deleted from the linked list.");
            Assert.IsNotNull(retrievedCharlie, "Charlie should still exist in the linked list.");
        }

        // 3. Custom MergeSort Algorithm Tests

        [TestMethod]
        public void MergeSort_SortByPriceAscending_ShouldOrderCorrectly()
        {
            // Arrange
            WembleyEvent[] events = new WembleyEvent[]
            {
                new WembleyEvent(1, 1, "A", DateTime.Now, "Football", 0, 300),
                new WembleyEvent(2, 1, "B", DateTime.Now, "Football", 0, 100),
                new WembleyEvent(3, 1, "C", DateTime.Now, "Football", 0, 500)
            };

            // Act
            EventSorter.MergeSort(events, "EventPrice", true);

            // Assert
            Assert.AreEqual(100, events[0].EventPrice, "First event should be the cheapest.");
            Assert.AreEqual(300, events[1].EventPrice);
            Assert.AreEqual(500, events[2].EventPrice, "Last event should be the most expensive.");
        }

        [TestMethod]
        public void MergeSort_SortByNameDescending_ShouldOrderCorrectly()
        {
            // Arrange
            WembleyEvent[] events = new WembleyEvent[]
            {
                new WembleyEvent(1, 1, "Apple", DateTime.Now, "Other", 0, 10),
                new WembleyEvent(2, 1, "Zebra", DateTime.Now, "Other", 0, 10),
                new WembleyEvent(3, 1, "Mango", DateTime.Now, "Other", 0, 10)
            };

            // Act
            EventSorter.MergeSort(events, "EventName", false);

            // Assert
            Assert.AreEqual("Zebra", events[0].EventName, "Zebra should be first in descending alphabetical order.");
            Assert.AreEqual("Mango", events[1].EventName);
            Assert.AreEqual("Apple", events[2].EventName);
        }
    }
}