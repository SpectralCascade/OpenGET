using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace OpenGET
{

    /// <summary>
    /// Implement this for own game if you want easy reflection-based serialisation with versioning.
    /// </summary>
    public abstract class SerialiseAttribute : Attribute
    {
        /// <summary>
        /// Serialisation version. It is recommended that you implement this as an enum in your derived class,
        /// which you update with a new version for every release build.
        /// </summary>
        public Enum version;

        /// <summary>
        /// Former name this variable was serialised as. Used to handle the renaming of a variable so old saves still work.
        /// </summary>
        public string formerName;

        /// <summary>
        /// Whether this variable has been removed from serialisation for this version.
        /// </summary>
        public bool removed;

        public SerialiseAttribute(Enum version, string formerName = null, bool removed = false)
        {
            this.version = version;
            this.formerName = formerName;
            this.removed = removed;
        }
    }

    public abstract class BaseSerialiser
    {
        /// <summary>
        /// Provides a unique identifier for referencing a temporary instance for serialisation (unlike persistent ids, which are used for assets).
        /// </summary>
        public sealed class Reference
        {
            public delegate string Generator();

            public Reference(Generator generator)
            {
                this.generator = generator;
            }

            private readonly Generator generator;

            public string id {
                get { return _id = (_id ?? generator.Invoke()); }
                set { _id = value; }
            }
            private string _id;
        }

        /// <summary>
        /// Implement this interface to convert references to and from objects for serialisation.
        /// </summary>
        public interface IReferenceSerialise
        {

            /// <summary>
            /// Reference identifier.
            /// </summary>
            public Reference reference { get; }

            /// <summary>
            /// Generate a unique reference identifier.
            /// </summary>
            public string GenerateReference();

        }

        /// <summary>
        /// Whether to serialise or deserialise data.
        /// </summary>
        public enum Mode
        {
            Serialise = 0,
            Deserialise = 1
        }

        /// <summary>
        /// Is this serialiser in serialising or deserialising mode?
        /// </summary>
        public Mode mode { get; protected set; }

        /// <summary>
        /// Whether to serialise or deserialise data.
        /// </summary>
        public bool isWriting => mode == Mode.Serialise;

        /// <summary>
        /// Whether to deserialise or serialise data.
        /// </summary>
        public bool isReading => !isWriting;

        /// <summary>
        /// Serialisation version.
        /// </summary>
        protected Enum version { get; set; }

        /// <summary>
        /// Serialise/deserialise a value (depending on serialiser mode).
        /// </summary>
        public void Serialise<DataType>(string id, ref DataType data, bool autoReference = true)
        {
            if (isWriting)
            {
                Write(id, data, autoReference && data is PersistentIdentity);
            }
            else
            {
                Read(id, ref data, autoReference && data is PersistentIdentity);
            }
        }

        /// <summary>
        /// Deserialise an object from the serialised format. Returns false if there is no such id in the current JSON data.
        /// </summary>
        public abstract bool Read<DataType>(string id, ref DataType data, bool asReference = false);

        /// <summary>
        /// Serialise the given data into a valid format for saving.
        /// </summary>
        public abstract void Write<DataType>(string id, DataType data, bool asReference = false);

    }

    /// <summary>
    /// Base class for implementing a custom serialisation system.
    /// </summary>
    public abstract class Serialiser<Derived, Version, VarAttribute> : BaseSerialiser where VarAttribute : SerialiseAttribute where Derived : Serialiser<Derived, Version, VarAttribute> where Version : Enum
    {
        /// <summary>
        /// Implement on classes you wish use custom serialisation logic for.
        /// </summary>
        public interface ISerialise
        {
            void Serialise(Derived s);
        }
        
        /// <summary>
        /// Path to the file to be serialised to/from.
        /// </summary>
        public virtual string path { get; protected set; }

        /// <summary>
        /// Current serialisation version.
        /// </summary>
        public new Version version => (Version)base.version;

        /// <summary>
        /// Total number of deserialisation phases.
        /// </summary>
        protected virtual int loadPhases => 1;

        /// <summary>
        /// Which phase of deserialisation is occurring.
        /// </summary>
        public int phase { get; protected set; }

        /// <summary>
        /// All registered objects that can be referenced from serialised data.
        /// </summary>
        protected readonly Dictionary<string, IReferenceSerialise> registeredObjects = new Dictionary<string, IReferenceSerialise>();

        /// <summary>
        /// Register an object that can be referenced from serialised data.
        /// </summary>
        public void RegisterObject<T>(T obj) where T : class, IReferenceSerialise
        {
            if (!registeredObjects.TryAdd(obj.reference.id, obj))
            {
                Log.Warning("Registered objects already contains reference with id \"{0}\"", obj.reference.id);
            }
        }

        /// <summary>
        /// Locate an object by reference id. Returns null on failure and throws an exception if the object was found but there is a type mismatch.
        /// </summary>
        public T FindReference<T>(string id) where T : class, IReferenceSerialise
        {
            T found = registeredObjects.TryGetValue(id, out IReferenceSerialise v) ? v as T : null;
            return found;
        }

        /// <summary>
        /// Clear all registered id references.
        /// </summary>
        public void Clear()
        {
            registeredObjects.Clear();
        }

        /// <summary>
        /// Serialise an object using it's custom serialisation handler.
        /// </summary>
        public virtual void Serialise(ISerialise data)
        {
            mode = Mode.Serialise;
            data.Serialise((Derived)this);
        }

        /// <summary>
        /// Deserialise an object using it's custom serialisation handler.
        /// </summary>
        public virtual void Deserialise(ISerialise data)
        {
            mode = Mode.Deserialise;
            data.Serialise((Derived)this);
        }

        /// <summary>
        /// Write to the file at path.
        /// </summary>
        public abstract Result Save(ISerialise obj);

        /// <summary>
        /// Read from the file at path.
        /// </summary>
        public abstract Result Load(ISerialise obj);

    }

}
