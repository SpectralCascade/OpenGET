using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Base class for implementing a custom serialisation system.
    /// </summary>
    public abstract class Serialiser<T, VersionType> where T : Serialiser<T, VersionType>
    {

        /// <summary>
        /// Implement on classes you wish use custom serialisation logic for.
        /// </summary>
        public interface ISerialise
        {
            /// <summary>
            /// Deserialise data.
            /// </summary>
            public void Deserialise(T serialiser);

            /// <summary>
            /// Serialise data.
            /// </summary>
            public void Serialise(T serialiser);

        }

        /// <summary>
        /// Path to the file to be serialised to/from.
        /// </summary>
        public virtual string path { get; protected set; }

        /// <summary>
        /// Current serialisation version.
        /// </summary>
        public virtual VersionType version { get; protected set; }

        /// <summary>
        /// Write to the file at path.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Read from the file at path.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Serialise the given data into a valid format for saving.
        /// </summary>
        public abstract void Serialise<DataType>(ref DataType data);

        /// <summary>
        /// Deserialise an object from the serialised format.
        /// </summary>
        public abstract void Deserialise<DataType>(ref DataType data);

    }

}
