using Codice.CM.WorkspaceServer.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Derive from this class to obtain
    /// </summary>
    public abstract class Polymorph
    {
        /// <summary>
        /// Get the deserialised object, if any.
        /// </summary>
        public Polymorph _deserialised { get; private set; }

        protected bool _serialLock { get; set; }

        [HideInInspector]
        [SerializeField]
        private string[] c = new string[0];

        [HideInInspector]
        [SerializeField]
        private string[] n = new string[0];

        [HideInInspector]
        [SerializeField]
        private string[] t = new string[0];

        /// <summary>
        /// Get all polymorph-derived fields.
        /// </summary>
        private FieldInfo[] PolymorphGetFields()
        {
            return GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => typeof(Polymorph).IsAssignableFrom(x.FieldType)).ToArray();
        }

        public string PolymorphSerialise(out Type type)
        {
            FieldInfo[] fields = PolymorphGetFields();
            c = new string[fields.Length];
            n = new string[fields.Length];
            t = new string[fields.Length];
            Log.Debug("Serialising {0} polymorph subfields.", c.Length);
            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                Log.Debug("Serialising field \"{0}\"...", fields[i].Name);
                Polymorph obj = fields[i].GetValue(this) as Polymorph;
                n[i] = fields[i].Name;
                t[i] = obj.GetType().FullName;
                c[i] = obj.PolymorphSerialise(out Type fieldType);
            }
            type = GetType();
            return JsonUtility.ToJson(this, false);
        }

        private void PolymorphDeserialise()
        {
            FieldInfo[] fields = PolymorphGetFields();
            Log.Debug("Deserialising {0} subfields...", fields.Length);
            for (int i = 0, counti = c.Length; i < counti; i++)
            {
                FieldInfo field = fields.FirstOrDefault(x => x.Name == n[i]);
                Polymorph child = null;
                if (field != null && !string.IsNullOrEmpty(c[i]))
                {
                    Log.Debug("Deserialising field \"{0}\"...", field.Name);
                    child = JsonUtility.FromJson(c[i], Type.GetType(t[i])) as Polymorph;
                    if (child != null)
                    {
                        field.SetValue(this, child);
                        child.PolymorphDeserialise();
                    }
                }
            }
            t = new string[0];
            n = new string[0];
            c = new string[0];
        }

        public static Polymorph PolymorphLoad(string json, string type)
        {
            Polymorph data = JsonUtility.FromJson(json, Type.GetType(type)) as Polymorph;
            data.PolymorphDeserialise();
            return data;
        }

    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SerializePolymorphAttribute : Attribute
    {
    }

}

