using System;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OpenGET
{

    // Dummy attribute used for marking groups of settings
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SettingsGroupAttribute : Attribute
    {

        public readonly string title;
        public readonly string description;

        public SettingsGroupAttribute(string title, string description = "")
        {
            this.title = title;
            this.description = description;
        }

    }

    /// <summary>
    /// Any setting that can be applied inherits this interface.
    /// </summary>
    public interface IApplySetting
    {
        void Apply();

        void Update(object data);

        bool HasDifference(IApplySetting other);

        object GetValue();

        void SetValue(object value);

        void SetValueRaw(object value);

        string GetName();

        string GetDescription();
    }

#if ENABLE_INPUT_SYSTEM
    public interface IActionMapBindings
    {
        public IInputActionCollection2 actions { get; }

        public abstract HashSet<string> enabledMaps { get; }

        public abstract bool showComposites { get; }

        public abstract string[] GetEnabledBindGroups();

        void UpdateAction(InputAction action);
    }
#endif

}