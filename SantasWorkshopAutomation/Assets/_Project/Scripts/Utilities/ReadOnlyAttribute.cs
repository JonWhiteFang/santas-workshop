using UnityEngine;

namespace SantasWorkshop.Utilities
{
    /// <summary>
    /// Attribute to make a serialized field read-only in the Inspector.
    /// Useful for debugging runtime state without allowing modification.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
}
