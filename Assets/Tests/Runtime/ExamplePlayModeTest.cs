using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SantasWorkshop.Tests.Runtime
{
    /// <summary>
    /// Example PlayMode test - runs in Play mode with full Unity runtime.
    /// Use for testing MonoBehaviour components, coroutines, physics, etc.
    /// </summary>
    public class ExamplePlayModeTest
    {
        [UnityTest]
        public IEnumerator Example_WaitForSeconds_WaitsCorrectly()
        {
            // Arrange
            float startTime = Time.time;

            // Act
            yield return new WaitForSeconds(0.1f);

            // Assert
            float elapsed = Time.time - startTime;
            Assert.GreaterOrEqual(elapsed, 0.1f, "Should wait at least 0.1 seconds");
        }

        [UnityTest]
        public IEnumerator Example_GameObject_CanBeInstantiated()
        {
            // Arrange & Act
            GameObject testObject = new GameObject("TestObject");
            testObject.AddComponent<Rigidbody>();

            // Wait one frame for physics initialization
            yield return null;

            // Assert
            Assert.IsNotNull(testObject, "GameObject should exist");
            Assert.IsNotNull(testObject.GetComponent<Rigidbody>(), "Rigidbody should be attached");

            // Cleanup
            Object.Destroy(testObject);
        }

        [UnityTest]
        public IEnumerator Example_Transform_MovesOverTime()
        {
            // Arrange
            GameObject testObject = new GameObject("MovingObject");
            Vector3 startPosition = Vector3.zero;
            testObject.transform.position = startPosition;

            // Act - Move object over multiple frames
            for (int i = 0; i < 10; i++)
            {
                testObject.transform.position += Vector3.right * 0.1f;
                yield return null;
            }

            // Assert
            Assert.Greater(testObject.transform.position.x, startPosition.x, "Object should have moved");
            Assert.AreEqual(1.0f, testObject.transform.position.x, 0.01f, "Object should be at x=1");

            // Cleanup
            Object.Destroy(testObject);
        }

        [Test]
        public void Example_MonoBehaviour_CanBeAdded()
        {
            // Arrange
            GameObject testObject = new GameObject("TestObject");

            // Act
            var component = testObject.AddComponent<TestMonoBehaviour>();

            // Assert
            Assert.IsNotNull(component, "Component should be added");
            Assert.IsTrue(component.enabled, "Component should be enabled");

            // Cleanup
            Object.Destroy(testObject);
        }

        [UnityTest]
        public IEnumerator Example_Coroutine_ExecutesCorrectly()
        {
            // Arrange
            GameObject testObject = new GameObject("TestObject");
            var component = testObject.AddComponent<TestMonoBehaviour>();

            // Act
            component.StartTestCoroutine();
            yield return new WaitForSeconds(0.2f);

            // Assert
            Assert.IsTrue(component.CoroutineExecuted, "Coroutine should have executed");

            // Cleanup
            Object.Destroy(testObject);
        }
    }

    /// <summary>
    /// Helper MonoBehaviour for testing
    /// </summary>
    public class TestMonoBehaviour : MonoBehaviour
    {
        public bool CoroutineExecuted { get; private set; }

        public void StartTestCoroutine()
        {
            StartCoroutine(TestCoroutine());
        }

        private IEnumerator TestCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            CoroutineExecuted = true;
        }
    }
}
