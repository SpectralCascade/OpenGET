using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OpenGET
{

    public abstract class SystemPathAttribute : PropertyAttribute
    {
        public readonly string title;

        public readonly string root;

        public SystemPathAttribute(string title, string root)
        {
            this.title = title;
            this.root = root;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FolderPathAttribute : SystemPathAttribute
    {
        public FolderPathAttribute(string root, string title = "Select Folder") : base(title, root) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FilePathAttribute : SystemPathAttribute
    {
        public string extension = "";

        public FilePathAttribute(string root, string extension, string title = "Select File") : base(title, root)
        {
            this.extension = extension;
        }
    }
    
}
