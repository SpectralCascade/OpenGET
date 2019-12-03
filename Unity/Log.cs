using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public static class Log
    {

        public static void Debug(string message, params object[] args) {
            UnityEngine.Debug.Log("<color=purple>Info: " + string.Format(message, args) + "</color>");
        }

        /// <summary>
        /// Logs information.
        /// </summary>
        public static void Info(string message, params object[] args) {
            UnityEngine.Debug.Log("<color=magenta>Info: " + string.Format(message, args) + "</color>");
        }

        /// <summary>
        /// Logs warnings.
        /// </summary>
        public static void Warning(string message, params object[] args) {
            UnityEngine.Debug.Log("<color=yellow>Warning: " + string.Format(message, args) + "</color>");
        }

        /// <summary>
        /// Logs errors.
        /// </summary>
        public static void Error(string message, params object[] args) {
            UnityEngine.Debug.Log("<color=red>Error: " + string.Format(message, args) + "</color>");
        }

        public static void Verbose(string message, params object[] args) {
            UnityEngine.Debug.Log("<color=cyan>Info: " + string.Format(message, args) + "</color>");
        }

        /// <summary>
        /// Logs exceptions.
        /// </summary>
        public static void Exception(Exception e) {
            UnityEngine.Debug.LogError(e.ToString());
        }

    }

}
