using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Implement this interface for asset types you want to be referrable.
    /// </summary>
    public interface IReferrable
    {
    }

    /// <summary>
    /// Implement this for ScriptableObjects you want to have serialisation support for. Prioritises assets in Resources.
    /// In future this may be extended to support Addressables.
    /// </summary>
    public abstract class Referrable : ScriptableObject, IReferrable
    {
        /// <summary>
        /// If not overridden, this returns the AssetPath.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Load a Referrable of the given type, given a reference path.
        /// </summary>
        public static T Load<T>(string reference) where T : Referrable
        {
            return Resources.Load<T>(reference);
        }

        /// <summary>
        /// Load a Referrable of the given type asynchronously, given a reference path.
        /// </summary>
        public static AsyncOperation LoadAsync<T>(string reference)
        {
            return Resources.LoadAsync(reference);
        }

    }

}
