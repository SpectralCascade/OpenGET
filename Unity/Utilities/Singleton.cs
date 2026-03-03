using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// An abstract singleton AutoBehaviour that persists within the "DontDestroyOnLoad" scene.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Singleton<Derived> : AutoBehaviour where Derived : Singleton<Derived>
    {
        private static Derived _sharedInstance;

        /// <summary>
        /// Singleton instance accessor for use in static methods.
        /// </summary>
        protected static Derived sharedInstance
        {
            get
            {
                if (_sharedInstance == null && Application.isPlaying)
                {
                    try
                    {
                        _sharedInstance = new GameObject(typeof(Derived).Name).AddComponent<Derived>();
                    }
                    catch
                    {
                        Log.Error("Failed to instantiate {0} singleton instance!", typeof(Derived).Name);
                    }
                }
                return _sharedInstance;
            }
            set
            {
                _sharedInstance = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_sharedInstance != null)
            {
                Log.Warning("More than one {0} instance exists! This should not happen. Destroying new instance...", typeof(Derived).Name);
                Destroy(_sharedInstance.gameObject);
            }
            _sharedInstance = this as Derived;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            // Just in case someone destroys this manually
            if (_sharedInstance == this)
            {
                _sharedInstance = null;
            }
        }

    }

}
