using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        public int version;

        /// <summary>
        /// Former name this variable was serialised as. Used to handle the renaming of a variable so old saves still work.
        /// </summary>
        public string formerName;

        /// <summary>
        /// Whether this variable has been removed from serialisation for this version.
        /// </summary>
        public bool removed;

        public SerialiseAttribute(int version, string formerName = null, bool removed = false)
        {
            this.version = version;
            this.formerName = formerName;
            this.removed = removed;
        }
    }

    /// <summary>
    /// Base class for implementing a custom serialisation system.
    /// </summary>
    public abstract class Serialiser<Derived, Version, VarAttribute> where VarAttribute : SerialiseAttribute where Derived : Serialiser<Derived, Version, VarAttribute>
    {

        /// <summary>
        /// Whether to serialise or deserialise data.
        /// </summary>
        public enum Mode
        {
            Serialise = 0,
            Deserialise = 1
        }

        /// <summary>
        /// Implement on classes you wish use custom serialisation logic for.
        /// </summary>
        public interface ISerialise
        {
            public void Serialise(Derived s);
        }

        /// <summary>
        /// Path to the file to be serialised to/from.
        /// </summary>
        public virtual string path { get; protected set; }

        /// <summary>
        /// Current serialisation version.
        /// </summary>
        public virtual Version version { get; protected set; }

        /// <summary>
        /// Is this serialiser in serialising or deserialising mode?
        /// </summary>
        public Mode mode { get; protected set; }

        /// <summary>
        /// Whether to serialise or deserialise data.
        /// </summary>
        public bool isWriting => mode == Mode.Serialise;

        /// <summary>
        /// Write to the file at path.
        /// </summary>
        public abstract Result Save(ISerialise game);

        /// <summary>
        /// Read from the file at path.
        /// </summary>
        public abstract Result Load(ISerialise game);

        /// <summary>
        /// Serialise/deserialise a value (depending on serialiser mode).
        /// </summary>
        public void Serialise<DataType>(string id, ref DataType data)
        {
            if (isWriting)
            {
                Write(id, data);
            }
            else
            {
                Read(id, ref data);
            }
        }

        /// <summary>
        /// Deserialise an object from the serialised format. Returns false if there is no such id in the current JSON data.
        /// </summary>
        public abstract bool Read<DataType>(string id, ref DataType data);

        /// <summary>
        /// Serialise the given data into a valid format for saving.
        /// </summary>
        public abstract void Write<DataType>(string id, DataType data);

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

    }

}