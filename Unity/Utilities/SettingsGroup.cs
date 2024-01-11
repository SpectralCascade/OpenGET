using System;

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

        bool HasDifference(IApplySetting other);

        object GetValue();

        void SetValue(object value);

        string GetName();

        string GetDescription();
    }

}