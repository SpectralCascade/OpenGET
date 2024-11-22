using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System;
using System.Linq;

namespace OpenGET
{

    /// <summary>
    /// JSON serialisation implementation.
    /// </summary>
    public abstract class SerialiserJSON<Derived, VersionType, VarAttribute> : Serialiser<Derived, VersionType, VarAttribute> where VarAttribute : SerialiseAttribute where Derived : SerialiserJSON<Derived, VersionType, VarAttribute>
    {
        protected JObject json = new JObject();

        protected delegate void HandleToken(JToken token);

        /// <summary>
        /// Ensures that properties are not serialised, a la Unity.
        /// </summary>
        protected class UnityContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                List<FieldInfo> members = new List<FieldInfo>();
                Type unityObjType = typeof(UnityEngine.Object);

                for (Type evalType = type; evalType != null; evalType = evalType.BaseType)
                {
                    FieldInfo[] fields = evalType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    for (int i = 0, counti = fields.Length; i < counti; i++)
                    {
                        // Don't serialise Unity Objects or explicitly non-serialised members
                        FieldInfo field = fields[i];
                        if (field.IsNotSerialized
                            || field.GetCustomAttribute<NonSerializedAttribute>() != null
                            || unityObjType.IsAssignableFrom(field.FieldType)
                            || (
                                !field.IsPublic
                                && field.GetCustomAttribute<SerializeField>() == null
                                && field.GetCustomAttribute<VarAttribute>() == null
                            )
                        )
                        {
                            continue;
                        }
                        members.Add(field);
                    }
                }

                members.Sort((a, b) => a.MetadataToken - b.MetadataToken);

                List<Newtonsoft.Json.Serialization.JsonProperty> properties = new List<Newtonsoft.Json.Serialization.JsonProperty>(members.Count);
                for (int i = 0, counti = members.Count; i < counti; i++)
                {
                    int index = properties.FindIndex((Newtonsoft.Json.Serialization.JsonProperty current) => current.UnderlyingName == members[i].Name);
                    if (index >= 0)
                    {
                        continue;
                    }
                    Newtonsoft.Json.Serialization.JsonProperty property = CreateProperty(members[i], memberSerialization);
                    property.Writable = true;
                    property.Readable = true;
                    properties.Add(property);
                }
                return properties;
            }
        }

        /// <summary>
        /// Serialiser using Unity-style contract resolver.
        /// </summary>
        protected JsonSerializer serial = new JsonSerializer();

        public SerialiserJSON()
        {
            serial.ContractResolver = new UnityContractResolver();
        }

        public override Result Save(ISerialise game)
        {
            json = new JObject();
            Serialise(game);

            // Attempt to convert the dictionaries to JSON
            string raw = json.ToString();
            try
            {
                File.WriteAllText(path, raw, System.Text.Encoding.UTF8);
                Log.Info("Saved to \"{0}\"", path);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return new Result(Localise.Text("Failed to save: {0}", e.Message), e);
            }
            return new Result();
        }

        public override Result Load(ISerialise game)
        {
            try
            {
                Log.Info("Loading from \"{0}\"", path);
                string raw = File.ReadAllText(path, System.Text.Encoding.UTF8);
                json = JObject.Parse(raw);

                Deserialise(game);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return new Result(Localise.Text("Failed to load: {0}", e.Message), e);
            }
            return new Result();
        }

        public delegate void HandleMember(FieldInfo field);

        /// <summary>
        /// Walk over members and selectively serialise them via reflection.
        /// </summary>
        protected void WalkSerialiseMembers(Type type, HandleMember handler)
        {
            // Use reflection to selectively choose what we serialise
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                FieldInfo field = fields[i];
                VarAttribute[] info = field.GetCustomAttributes<VarAttribute>().ToArray();
                if (!(info.Length > 0 && info.Last().removed) &&
                    (
                        (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() == null)
                        || field.GetCustomAttribute<SerializeField>() != null
                        || field.GetCustomAttribute<VarAttribute>() != null
                    )
                ) {
                    handler(field);
                }
            }
        }
        
        /// <summary>
        /// Walk over members and selectively serialise them via reflection.
        /// </summary>
        protected void WalkReadMembers<DataType>(Type type, ref DataType data)
        {
            // Use reflection to selectively choose what we serialise
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                FieldInfo field = fields[i];
                VarAttribute[] info = field.GetCustomAttributes<VarAttribute>().ToArray();
                if (!(info.Length > 0 && info.Last().removed) &&
                    (
                        (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() == null)
                        || field.GetCustomAttribute<SerializeField>() != null
                        || field.GetCustomAttribute<VarAttribute>() != null
                    )
                ) {
                    object member = field.GetValue(data);
                    Read(field.Name, ref member);
                    field.SetValue(data, member);
                }
            }
        }

        public override bool Read<DataType>(string id, ref DataType data)
        {
            // Make sure custom serialisation runs
            if (!json.ContainsKey(id))
            {
                return false;
            }

            //Log.Debug($"Reading JSON entry \"{id}\"");
            if (data is ISerialise custom)
            {
                JToken token = json[id];
                if (token is not JObject)
                {
                    Log.Warning("Type mismatch: JSON token is not an ISerialise JSON object!");
                    return false;
                }
                JObject prev = json;
                json = (JObject)token;
                custom.Serialise((Derived)this);
                json = prev;
            }
            else // TODO: Provide class attribute that ensures we ADDITIONALLY do this step for ISerialise implementations
            {
                JToken token = json[id];
                if (token.Type == JTokenType.Object)
                {
                    if (token is not JObject)
                    {
                        Log.Warning("Type mismatch: JSON token is not a JSON object!");
                        return false;
                    }
                    // Nested object
                    JObject prev = json;
                    json = (JObject)token;

                    // Use reflection to selectively choose what we serialise
                    WalkReadMembers(data.GetType(), ref data);

                    if (data is IDictionary dict)
                    {
                        Type[] types = dict.GetType().GetGenericArguments();
                        if (types.Length > 0 && types[0].IsAssignableFrom(typeof(string)))
                        {
                            IEnumerable<string> keys = json.Properties().Select(x => x.Name);
                            foreach (string key in keys)
                            {
                                Read(key, ref data);
                            }
                        }
                    }

                    json = prev;
                }
                else if (token.Type == JTokenType.Array && data is IList)
                {
                    void HandleArray(JArray jArray, Type arrayType, ref object array)
                    {
                        // Create a brand new array and populate it
                        array = Activator.CreateInstance(arrayType, 0);
                        Type itemType = arrayType.GetElementType();
                        foreach (JToken child in jArray)
                        {
                            if (child.Type == JTokenType.Array)
                            {
                                if (itemType.GetInterface("IList") == null)
                                {
                                    // Can't handle non-IList implementations
                                    continue;
                                }
                                object nested = null;
                                HandleArray((JArray)child, itemType, ref nested);
                                ((IList)array).Add(nested);
                            }
                            else if (itemType.IsAssignableFrom(typeof(ISerialise)))
                            {
                                JObject prev = json;
                                json = (JObject)child;
                                object created = Activator.CreateInstance(itemType);
                                ((IList)array).Add(created);
                                ((ISerialise)created).Serialise((Derived)this);
                                json = prev;
                            }
                            else if (child.Type == JTokenType.Object)
                            {
                                JObject prev = json;
                                json = (JObject)child;
                                object created = Activator.CreateInstance(itemType);
                                WalkSerialiseMembers(itemType, (field) => {
                                    Read(field.Name, ref created);
                                });
                                ((IList)array).Add(created);
                                json = prev;
                            }
                            else
                            {
                                ((IList)array).Add(child.ToObject(itemType));
                            }
                        }
                    }

                    object dataObj = data;
                    HandleArray((JArray)token, typeof(DataType), ref dataObj);
                    data = (DataType)dataObj;
                }
                else
                {
                    data = (DataType)token.ToObject(data.GetType());
                }
            }
            return true;
        }

        public override void Write<DataType>(string id, DataType data)
        {
            // Make sure custom serialisation runs
            if (data == null)
            {
                return;
            }
            else if (data is ISerialise custom)
            {
                JObject prev = json;
                json = new JObject();
                prev.Add(id, json);
                custom.Serialise((Derived)this);
                json = prev;
            }
            else // TODO: Provide class attribute that ensures we ADDITIONALLY do this step for ISerialise implementations
            {
                JToken token = JToken.FromObject(data, serial);
                if (token.Type == JTokenType.Object)
                {
                    // Nested object
                    JObject prev = json;
                    json = new JObject();
                    prev.Add(id, json);

                    // Use reflection to selectively choose what we serialise
                    WalkSerialiseMembers(data.GetType(), (field) => {
                        DataType scoped = data;
                        Write(field.Name, field.GetValue(scoped));
                    });

                    if (data is IDictionary dict)
                    {
                        IEnumerable<string> keys = dict.Keys.OfType<string>();
                        if (keys.Count() > 0)
                        {
                            foreach (string key in keys)
                            {
                                Write(key, dict[key]);
                            }
                        }
                    }

                    json = prev;
                }
                else if (token.Type == JTokenType.Array && data is IEnumerable)
                {
                    void HandleArray(JArray jArray, IEnumerable array)
                    {
                        foreach (object child in array)
                        {
                            JToken arrayToken = JToken.FromObject(child, serial);
                            if (child == null)
                            {
                            }
                            else if (arrayToken.Type == JTokenType.Array && child is IEnumerable)
                            {
                                JArray nested = new JArray();
                                jArray.Add(nested);
                                HandleArray(nested, child as IEnumerable);
                            }
                            else if (child is ISerialise custom)
                            {
                                JObject prev = json;
                                json = new JObject();
                                jArray.Add(json);
                                custom.Serialise((Derived)this);
                                json = prev;
                            }
                            else if (arrayToken.Type == JTokenType.Object)
                            {
                                JObject prev = json;
                                json = new JObject();
                                jArray.Add(json);
                                WalkSerialiseMembers(child.GetType(), (field) => {
                                    object scoped = child;
                                    Write(field.Name, field.GetValue(scoped));
                                });
                                json = prev;
                            }
                            else
                            {
                                jArray.Add(arrayToken);
                            }
                        }
                    }

                    JArray jArray = new JArray();
                    json.Add(id, jArray);
                    HandleArray(jArray, data as IEnumerable);
                }
                else
                {
                    json.Add(id, token);
                }
            }
        }
        
    }

}
