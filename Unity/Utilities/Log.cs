using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public static class Log
    {

        public static void Debug(string message, params object[] args) {
            UnityEngine.Debug.Log(
#if UNITY_EDITOR
                "<color=purple>Info: " + 
#endif
                string.Format(message, args) 
#if UNITY_EDITOR
                + "</color>"
#endif
            );
        }

        /// <summary>
        /// Logs information.
        /// </summary>
        public static void Info(string message, params object[] args) {
            UnityEngine.Debug.Log(
#if UNITY_EDITOR
                "<color=magenta>Info: " + 
#endif
                string.Format(message, args) 
#if UNITY_EDITOR            
                + "</color>"
#endif
            );
        }

        /// <summary>
        /// Logs warnings.
        /// </summary>
        public static void Warning(string message, params object[] args) {
            UnityEngine.Debug.Log(
#if UNITY_EDITOR
                "<color=yellow>Warning: " + 
#endif
                string.Format(message, args)
#if UNITY_EDITOR
                + "</color>"
#endif
            );
        }

        /// <summary>
        /// Logs errors.
        /// </summary>
        public static void Error(string message, params object[] args) {
            UnityEngine.Debug.Log(
#if UNITY_EDITOR
                "<color=red>Error: " + 
#endif
                string.Format(message, args)
#if UNITY_EDITOR
                + "</color>"
#endif
            );
        }

        public static void Verbose(string message, params object[] args) {
            UnityEngine.Debug.Log(
#if UNITY_EDITOR
                "<color=cyan>Info: " + 
#endif
                string.Format(message, args)
#if UNITY_EDITOR
                + "</color>"
#endif
            );
        }

        /// <summary>
        /// Logs exceptions.
        /// </summary>
        public static void Exception(Exception e) {
            UnityEngine.Debug.LogError(e.ToString());
        }

    }

}
