using System;

namespace SantasWorkshop.Data
{
    /// <summary>
    /// Represents a quantity of a specific resource.
    /// Used in recipes, inventories, and resource transfers.
    /// </summary>
    [Serializable]
    public struct ResourceStack
    {
        /// <summary>
        /// The unique identifier of the resource type.
        /// </summary>
        public string resourceId;
        
        /// <summary>
        /// The quantity of this resource.
        /// </summary>
        public int amount;
        
        /// <summary>
        /// Creates a new resource stack.
        /// </summary>
        /// <param name="id">The resource identifier.</param>
        /// <param name="qty">The quantity.</param>
        public ResourceStack(string id, int qty)
        {
            resourceId = id;
            amount = qty;
        }
        
        /// <summary>
        /// Returns a string representation of this resource stack.
        /// </summary>
        public override string ToString()
        {
            return $"{resourceId} x{amount}";
        }
    }
}
