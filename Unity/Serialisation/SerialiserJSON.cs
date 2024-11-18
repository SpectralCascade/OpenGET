using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace OpenGET
{

    /// <summary>
    /// JSON serialisation implementation.
    /// </summary>
    public abstract class SerialiserJSON<T, VersionType> : Serialiser<SerialiserJSON<T, VersionType>, VersionType> where T : SerialiserJSON<T, VersionType>
    {
        /// <summary>
        /// Serialised JSON data.
        /// </summary>
        protected Dictionary<string, string> data = new Dictionary<string, string>();
        
        public override void Save()
        {
            
        }

        public override void Load()
        {
        }

        public override void Serialise<DataType>(ref DataType data)
        {
        }

        public override void Deserialise<DataType>(ref DataType data)
        {
        }

    }

}
