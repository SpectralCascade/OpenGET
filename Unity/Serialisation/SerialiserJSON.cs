using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System;
using System.Linq;
using CSVFile;

namespace OpenGET
{

    /// <summary>
    /// JSON serialisation implementation.
    /// </summary>
    public abstract class SerialiserJSON<Derived, VersionType, VarAttribute> : Serialiser<Derived, VersionType, VarAttribute> where VarAttribute : SerialiseAttribute where Derived : SerialiserJSON<Derived, VersionType, VarAttribute> where VersionType : Enum
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
#if !UNITY_EDITOR
            try
#endif
            {
                Log.Info("Loading from \"{0}\"", path);
                string raw = File.ReadAllText(path, System.Text.Encoding.UTF8);
                json = JObject.Parse(raw);

                Deserialise(game);
            }
#if !UNITY_EDITOR
            catch (Exception e)
            {
                Log.Exception(e);
                return new Result(Localise.Text("Failed to load: {0}", e.Message), e);
            }
#endif
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
                    Log.Debug("Reading field {0}", field.Name);
                    object member = field.GetValue(data);
                    if (member == null)
                    {
                        member = Activator.CreateInstance(field.FieldType);
                    }
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

                    if (data != null)
                    {
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
                    }

                    json = prev;
                }
                else if (token.Type == JTokenType.Array && data is IList)
                {
                    void HandleArray(JArray jArray, Type arrayType, ref object array)
                    {
                        Log.Debug($"Handling JArray with {jArray.Count} elements: {jArray}");

                        // Create a brand new array and populate it
                        array = Activator.CreateInstance(arrayType, jArray.Count);
                        Type itemType = arrayType.IsArray ? arrayType.GetElementType() : arrayType.GetGenericArguments().FirstOrDefault();
                        if (itemType == null)
                        {
                            Log.Error("Failed to determine element type of IList {0} (IList type: {1}) when reading data!", id, arrayType);
                        }
                        int index = 0;
                        IList casted = (IList)array;
                        void AddToArray(ref object obj)
                        {
                            if (arrayType.IsArray)
                            {
                                casted[index] = obj;
                            }
                            else
                            {
                                casted.Add(obj);
                            }
                        }

                        foreach (JToken element in jArray)
                        {
                            if (element.Type == JTokenType.Array)
                            {
                                if (itemType.GetInterface("IList") == null)
                                {
                                    // Can't handle non-IList implementations
                                    continue;
                                }
                                object loaded = null;
                                HandleArray((JArray)element, itemType, ref loaded);
                                AddToArray(ref loaded);
                            }
                            else if (itemType.IsAssignableFrom(typeof(ISerialise)))
                            {
                                JObject prev = json;
                                json = (JObject)element;
                                object loaded = Activator.CreateInstance(itemType);
                                AddToArray(ref loaded);
                                ((ISerialise)loaded).Serialise((Derived)this);
                                json = prev;
                            }
                            else if (element.Type == JTokenType.Object)
                            {
                                JObject prev = json;
                                json = (JObject)element;
                                Log.Debug("Reading element of type {0}", itemType);
                                object created = Activator.CreateInstance(itemType);
                                WalkReadMembers(itemType, ref created);
                                AddToArray(ref created);
                                json = prev;
                            }
                            else
                            {
                                object loaded = element.ToObject(itemType);
                                AddToArray(ref loaded);
                            }

                            index++;
                        }
                    }

                    object dataObj = data;
                    HandleArray((JArray)token, data.GetType(), ref dataObj);
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
                else if (token.Type == JTokenType.Array && data is IList)
                {
                    void HandleArray(JArray jArray, IList array)
                    {
                        foreach (object child in array)
                        {
                            if (child == null)
                            {
                                continue;
                            }

                            JToken arrayToken = JToken.FromObject(child, serial);
                            if (arrayToken.Type == JTokenType.Array && child is IList)
                            {
                                JArray nested = new JArray();
                                jArray.Add(nested);
                                HandleArray(nested, child as IList);
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
                    HandleArray(jArray, data as IList);
                }
                else
                {
                    json.Add(id, token);
                }
            }
        }
        
    }

}
