using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace OpenGET
{
    /// <summary>
    /// Individual settings should use this wrapper class so they can be applied in some way once a value is selected.
    /// </summary>
    [Serializable]
    public struct Setting<T> : IApplySetting
    {
        public delegate void ApplySetting(T current);

        public delegate string GetText();

        /// <summary>
        /// Create a new setting.
        /// </summary>
        /// <param name="value">The initial setting value.</param>
        /// <param name="apply">Callback for applying the setting.</param>
        /// <param name="previewOnChange">Whether this setting be applied for preview immediately upon changing it.</param>
        /// <param name="autoRevertTime">
        /// Show a confirmation dialog that auto-reverts the setting if the specified time in seconds elapses.
        /// If a time of zero or less is speciified, no confirmation dialog or auto-revert takes place for this setting.
        /// Please note this option is only adhered to for settings that are applied for preview on change.
        /// </param>
        /// <param name="name">Get a text string to use as the setting name. If null, uses member name.</param>
        /// <param name="desc">Get a text string to use as the setting description.</param>
        /// <param name="customData">Custom non-serialised data that can be accessed & changed dynamically at runtime.</param>
        public Setting(
            T value = default,
            ApplySetting apply = null,
            bool previewOnChange = false,
            float autoRevertTime = 0,
            GetText name = null,
            GetText desc = null,
            object[] customData = null
        )
        {
            this.value = value;
            this.apply = apply;
            this.previewOnChange = previewOnChange;
            this.autoRevertTime = autoRevertTime;
            this.getName = name;
            this.getDescription = desc;
            this.customData = customData;
        }

        public static implicit operator T(Setting<T> s)
        {
            return s.value;
        }

        public static implicit operator Setting<T>(T s)
        {
            return new Setting<T>(s);
        }

        /// <summary>
        /// The setting value to be serialised.
        /// </summary>
        [SerializeField]
        private T value;

        /// <summary>
        /// Function used to apply the given setting.
        /// </summary>
        private ApplySetting apply;

        /// <summary>
        /// Should this setting be immediately applied for preview when it is changed?
        /// </summary>
        public readonly bool previewOnChange;

        /// <summary>
        /// Time to elapse before the preview is automatically reverted.
        /// </summary>
        public readonly float autoRevertTime;

        /// <summary>
        /// Get the name of this setting to use as UI text.
        /// </summary>
        private GetText getName;

        /// <summary>
        /// Get the description of this setting to use as UI text.
        /// </summary>
        private GetText getDescription;

        /// <summary>
        /// Custom data that is not serialised.
        /// </summary>
        [NonSerialized]
        public object[] customData;

        /// <summary>
        /// Apply the setting.
        /// </summary>
        public void Apply()
        {
            apply?.Invoke(value);
        }

        /// <summary>
        /// Check if the setting has a different value to another.
        /// </summary>
        public bool HasDifference(IApplySetting other)
        {
            return !((Setting<T>)other).value.Equals(value);
        }

        /// <summary>
        /// Get the setting value as an object. This is not recommended for general use.
        /// </summary>
        public object GetValue()
        {
            return value;
        }

        /// <summary>
        /// Set the setting value as an object. This is not recommended for general use.
        /// </summary>
        public void SetValue(object value)
        {
            bool changed = !this.value.Equals((T)value);
            this.value = (T)value;
            // Apply this setting temporarily if preview is expected
            if (changed && previewOnChange)
            {
                apply?.Invoke(this.value);
            }
        }

        /// <summary>
        /// Callback setting name.
        /// </summary>
        public string GetName()
        {
            return getName?.Invoke();
        }

        /// <summary>
        /// Callback setting description.
        /// </summary>
        public string GetDescription()
        {
            return getDescription?.Invoke();
        }
    }

    /// <summary>
    /// Stores the game settings. Derive from this class using the CRTP pattern, e.g.
    /// <code>public sealed class MySettings : Settings&lt;MySettings&gt;</code>
    /// The OpenGET.Bootstrap namespace contains at least one derivative class with common settings implemented.
    /// </summary>
    [Serializable]
    public class Settings<Derived> where Derived : Settings<Derived>, new()
    {

        /// <summary>
        /// The current active settings.
        /// </summary>
        private static Derived instance = null;

        /// <summary>
        /// The settings data that was last applied.
        /// </summary>
        private static Derived applied = null;

        /// <summary>
        /// Retrieve the path to the associated settings file.
        /// </summary>
        public virtual string GetPath()
        {
            return Application.persistentDataPath + "/" + "settings.json";
        }

        /// <summary>
        /// Retrieve the shared instance path to the associated settings file.
        /// </summary>
        public static string Path => shared.GetPath();

        /// <summary>
        /// Get all members in the derived instance that have the Group attribute.
        /// </summary>
        protected List<object> GetGroups()
        {
            return GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public
            ).Where(
                x => Attribute.IsDefined(x, typeof(SettingsGroupAttribute))
            ).Select(
                x => x.GetValue(this)
            ).ToList();
        }

        /// <summary>
        /// Get all members in the derived instance that have the Group attribute.
        /// Overload that provides additional meta data about the groups.
        /// </summary>
        protected List<object> GetGroups(out SettingsGroupAttribute[] meta)
        {
            IEnumerable<FieldInfo> fields = GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public
            ).Where(
                x => Attribute.IsDefined(x, typeof(SettingsGroupAttribute))
            );
            
            meta = fields.Select(
                x => x.GetCustomAttribute(typeof(SettingsGroupAttribute)) as SettingsGroupAttribute
            ).ToArray();

            return fields.Select(x => x.GetValue(this)).ToList();
        }

        /// <summary>
        /// Copy the field values in all groups of another settings instance into this instance, using reflection.
        /// </summary>
        public void CopyIn(Derived source)
        {
            List<object> sourceGroups = source.GetGroups();
            List<object> instanceGroups = GetGroups();
            for (int i = 0, counti = sourceGroups.Count; i < counti; i++)
            {
                FieldInfo[] fields = sourceGroups[i].GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                for (int fieldIndex = 0, totalFields = fields.Length; fieldIndex < totalFields; fieldIndex++)
                {
                    // Overwrite this instance's value
                    fields[fieldIndex].SetValue(instanceGroups[i], fields[fieldIndex].GetValue(sourceGroups[i]));
                }
            }
        }

        /// <summary>
        /// Overwrite the settings saved on file with the current applied settings.
        /// </summary>
        public static Result Save(string customPath = "")
        {
            CheckInit();
            try
            {
                // Attempt with applied settings, if never applied then use current instance instead
                string json = JsonUtility.ToJson(applied ?? instance);
                File.WriteAllText(string.IsNullOrEmpty(customPath) ? instance.GetPath() : customPath, json);
            }
            catch (Exception e)
            {
                return new Result("Failed to save settings.", e);
            }
            return new Result();
        }

        /// <summary>
        /// Load settings from file. Returns false upon failure.
        /// </summary>
        public static Result Load(string customPath = "")
        {
            CheckInit();
            try
            {
                string json = File.ReadAllText(string.IsNullOrEmpty(customPath) ? Path : customPath);
                JsonUtility.FromJsonOverwrite(json, instance);
            }
            catch (Exception e)
            {
                return new Result("Failed to load settings.", e);
            }
            Log.Debug("Loaded settings successfully.");
            return new Result();
        }

        /// <summary>
        /// Check if the shared settings are different to applied settings.
        /// </summary>
        public static bool HasChanges()
        {
            if (applied == null)
            {
                Apply();
            }

            // Get settings groups using reflection
            List<object> groups = instance.GetGroups();
            List<object> appliedGroups = applied.GetGroups();
            for (int i = 0, counti = groups.Count; i < counti; i++)
            {
                // Get all IApply fields
                FieldInfo[] fields = groups[i].GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public
                ).Where(
                    f => typeof(IApplySetting).IsAssignableFrom(f.FieldType)
                ).ToArray();

                // Compare the values of the fields between the shared instance and the applied instance.
                for (int j = 0, countj = fields.Length; j < countj; j++)
                {
                    IApplySetting field = fields[j].GetValue(groups[i]) as IApplySetting;
                    IApplySetting current = fields[j].GetValue(appliedGroups[i]) as IApplySetting;
                    if (field.HasDifference(current))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Apply shared settings.
        /// </summary>
        public static bool Apply(bool force = false)
        {
            CheckInit();

            bool firstTime = applied == null;
            if (firstTime)
            {
                Log.Debug("First time, creating new applied settings...");
                applied = new Derived();
            }

            bool changed = false;

            List<object> groups = instance.GetGroups(out SettingsGroupAttribute[] meta);
            List<object> appliedGroups = applied.GetGroups();
            for (int i = 0, counti = groups.Count; i < counti; i++)
            {
                // Get all IApply fields
                FieldInfo[] fields = groups[i].GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public
                ).Where(
                    f => typeof(IApplySetting).IsAssignableFrom(f.FieldType)
                ).ToArray();

                //Log.Debug("Found {0} settings to check in group {1}", fields.Length, meta[i].title);
                for (int j = 0, countj = fields.Length; j < countj; j++)
                {
                    IApplySetting settingCurrent = fields[j].GetValue(groups[i]) as IApplySetting;
                    IApplySetting settingApplied = fields[j].GetValue(appliedGroups[i]) as IApplySetting;
                    //Log.Debug("Setting {0} current: {1}, applied: {2}", fields[j].Name, settingCurrent.GetValue(), settingApplied.GetValue());
                    if (settingCurrent.HasDifference(settingApplied) || firstTime || force)
                    {
                        settingCurrent.Apply();
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                // Copy settings to indicate they have been applied.
                applied.CopyIn(shared);
            }

            return changed;
        }

        /// <summary>
        /// Revert shared settings to last applied values.
        /// </summary>
        public static void Revert(Derived previous = null)
        {
            if (previous == null && applied == null)
            {
                Debug.LogError("Attempted to revert settings, but they've never been applied!");
            }
            else
            {
                // Use specified "previous" settings when available
                shared.CopyIn(previous ?? applied);
                Apply(force: true);
            }
        }

        /// <summary>
        /// Get: Retrieve reference to the shared settings instance (automatically initialises if first time).
        /// Set: Copy the settings across to the shared instance.
        /// </summary>
        public static Derived shared {
            get {
                CheckInit();
                return instance;
            }
            set {
                // Copy across settings, using reflection
                CheckInit();
                instance.CopyIn(value);
            }
        }

        /// <summary>
        /// Called when a new settings file is about to be created.
        /// </summary>
        protected virtual void OnFileCreate()
        {
        }

        /// <summary>
        /// Called when settings are about to be applied after being loaded for the first time.
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// Ensure the singleton instance is initialised and pre-existing settings are applied.
        /// </summary>
        private static void CheckInit()
        {
            if (instance == null)
            {
                // First time using settings on this run, try to load
                instance = new Derived();
                Result result = Load();
                if (result.hasException && result.exception.GetType() == typeof(FileNotFoundException))
                {
                    // First time ever running or settings file has been deleted, write a new settings file
                    Log.Debug("Settings file does not exist, writing default settings...");
                    instance.OnFileCreate();
                    result = Save();
                }

                if (result.hasError)
                {
                    // Something has gone wrong. Maybe we don't have permission to write files.
                    Log.Error("Cannot initialise settings due to error: {0} Exception: {1}", result.error, result.exception);
                }
                else
                {
                    instance.OnLoad();
                    Apply();
                    Log.Debug("Successfully initialised settings instance.");
                }
            }
        }

    }

}
