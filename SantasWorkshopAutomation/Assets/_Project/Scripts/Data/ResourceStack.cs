using System;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// Represents a stack of resources with an ID and amount.
    /// Used throughout the game for resource transactions, recipes, and storage.
    /// </summary>
    [Serializable]
    public struct ResourceStack
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        public string resourceId;

        /// <summary>
        /// The amount of the resource in this stack.
        /// </summary>
        public int amount;

        /// <summary>
        /// Creates a new resource stack.
        /// </summary>
        /// <param name="id">The resource identifier</param>
        /// <param name="amt">The amount of the resource</param>
        public ResourceStack(string id, int amt)
        {
            resourceId = id;
            amount = amt;
        }

        /// <summary>
        /// Returns a string representation of this resource stack.
        /// </summary>
        public override string ToString()
        {
            return $"{resourceId} x{amount}";
        }

        /// <summary>
        /// Checks if this resource stack is valid (has an ID and positive amount).
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(resourceId) && amount > 0;
        }
    }
}
