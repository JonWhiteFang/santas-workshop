using NUnit.Framework;
using UnityEngine;

namespace SantasWorkshop.Tests.Editor
{
    /// <summary>
    /// Example EditMode test - runs in the Unity Editor without entering Play mode.
    /// Use for testing logic that doesn't require runtime behavior.
    /// </summary>
    public class ExampleEditModeTest
    {
        [Test]
        public void Example_BasicAssertion_Passes()
        {
            // Arrange
            int expected = 5;
            int actual = 2 + 3;

            // Act & Assert
            Assert.AreEqual(expected, actual, "Basic math should work");
        }

        [Test]
        public void Example_GameObjectCreation_CreatesObject()
        {
            // Arrange & Act
            GameObject testObject = new GameObject("TestObject");

            // Assert
            Assert.IsNotNull(testObject, "GameObject should be created");
            Assert.AreEqual("TestObject", testObject.name, "GameObject should have correct name");

            // Cleanup
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void Example_VectorMath_CalculatesCorrectly()
        {
            // Arrange
            Vector3 a = new Vector3(1, 2, 3);
            Vector3 b = new Vector3(4, 5, 6);

            // Act
            Vector3 result = a + b;

            // Assert
            Assert.AreEqual(new Vector3(5, 7, 9), result, "Vector addition should work correctly");
        }

        [Test]
        public void Example_StringManipulation_WorksAsExpected()
        {
            // Arrange
            string input = "Santa's Workshop";

            // Act
            string result = input.ToUpper();

            // Assert
            Assert.AreEqual("SANTA'S WORKSHOP", result, "String should be uppercase");
            Assert.IsTrue(result.Contains("SANTA"), "Result should contain SANTA");
        }

        [Test]
        public void Example_CollectionTest_ManipulatesCorrectly()
        {
            // Arrange
            var list = new System.Collections.Generic.List<int> { 1, 2, 3 };

            // Act
            list.Add(4);
            list.Remove(2);

            // Assert
            Assert.AreEqual(3, list.Count, "List should have 3 items");
            Assert.Contains(4, list, "List should contain 4");
            Assert.IsFalse(list.Contains(2), "List should not contain 2");
        }
    }
}
