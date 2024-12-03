using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OpenGET {

    /// <summary>
    /// Utility class for logging debug information. Use this instead of Unity's Debug.Log()!
    /// You can also filter and add listeners to different log levels.
    /// </summary>
    public static class Log {

        /// <summary>
        /// Anything that wants to listen to logging events should implement this method and be added to the log.
        /// </summary>
        public interface ILogger {
            void OnLog(Level level, string message, params object[] args);
        }

        /// <summary>
        /// Unity debug logger implementation.
        /// </summary>
        protected class UnityLogger : ILogger
        {
            public void OnLog(Level level, string message, params object[] args)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorPrefs.HasKey("OpenGET/LogLevel"))
                {
                    UnityEditor.EditorPrefs.SetInt("OpenGET/LogLevel", (int)Level.All);
                }
                else if ((UnityEditor.EditorPrefs.GetInt("OpenGET/LogLevel") & (int)level) == 0)
                {
                    return;
                }
#endif
                LogType type = level switch {
                    _ => LogType.Log
                };
                string colour = level switch {
                    Level.Verbose => "green",
                    Level.Debug => "cyan",
                    Level.Info => "white",
                    Level.Warning => "yellow",
                    Level.Error => "red",
                    _ => null
                };
                if (level == Level.Error)
                {
                    // Log as error
                    LogLine(PrefixStackInfo(Format(null, message, args)), LogType.Error);
#if !UNITY_EDITOR
                    // Early out of builds so we only log errors once
                    return;
#endif
                }
                // Log as standard
                LogLine(PrefixStackInfo(Format(colour, message, args)), type);
            }
        }

        /// <summary>
        /// Static instance of the Unity logger.
        /// </summary>
        private static UnityLogger unity = new UnityLogger();

        /// <summary>
        /// Log priority levels.
        /// </summary>
        [System.Flags]
        public enum Level {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 4,
            Debug = 8,
            Verbose = 16,
            All = 31
        }

        /// <summary>
        /// All subscribed loggers.
        /// </summary>
        private static Dictionary<ILogger, Level> loggers = new Dictionary<ILogger, Level>();

        /// <summary>
        /// Add a logger to listen to logging events.
        /// </summary>
        public static void AddLogger(ILogger logger, Level level = Level.All) {
            loggers.Add(logger, level);
        }

        /// <summary>
        /// Remove a logger so it no longer listens to logging events.
        /// </summary>
        public static void RemoveLogger(ILogger logger) {
            loggers.Remove(logger);
        }

        /// <summary>
        /// Set the log level of a logger that has already been registered to listen to logging events.
        /// Returns false if the logger has not been registered and so the log level cannot be set.
        /// </summary>
        public static bool SetLogLevel(ILogger logger, Level level) {
            if (loggers.ContainsKey(logger)) {
                loggers[logger] = level;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calls the OnLog method of all ILogger instances registered as listeners.
        /// </summary>
        private static void UpdateLoggers(Level level, string message, params object[] args) {
#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.HasKey("OpenGET/LogLevel"))
            {
                UnityEditor.EditorPrefs.SetInt("OpenGET/LogLevel", (int)Level.All);
            }
            if ((UnityEditor.EditorPrefs.GetInt("OpenGET/LogLevel") & (int)level) != 0)
            {
#endif
                foreach (KeyValuePair<ILogger, Level> logger in loggers) {
                    if ((logger.Value & level) != 0) {
                        logger.Key.OnLog(level, message, args);
                    }
                }
#if UNITY_EDITOR
            }
#endif
        }

        /// <summary>
        /// Format the log console output.
        /// </summary>
        public static string Format(string colour, string message, params object[] args) {
            string formatted = "";
            if (args.Length <= 0) {
                formatted = message;
            } else {
                // Test format; if misused we should log an exception but not explode.
                try {
                    formatted = string.Format(message, args);
                } catch (System.Exception error) {
#if OPENGET_DEBUG
                    Log.Warning(
                        "Bad string format. Please use string formatters, not concatenation: \"{0}\" with exception: {1}",
                        message,
                        error
                    );
#endif
                    formatted = message;
                }
            }

            // Return colour formatted in editor
            bool hasColour = !string.IsNullOrEmpty(colour);
            return
#if UNITY_EDITOR
            (hasColour ? "<color=" + colour + ">" : "") +
#endif
            formatted
#if UNITY_EDITOR
            + (hasColour ? "</color>" : "")
#endif
            ;
        }

        /// <summary>
        /// Prefix stack information such as the file name and line number of the originating Log.X() call.
        /// This is only done in editor.
        /// </summary>
        public static string PrefixStackInfo(string message) {
#if UNITY_EDITOR
            System.Diagnostics.StackFrame caller = new System.Diagnostics.StackTrace(true).GetFrames().FirstOrDefault(
                x => x != null && System.IO.Path.GetFileNameWithoutExtension(x.GetFileName()) != "Log"
            );
            message = "<color=#AFA>[" + 
                System.IO.Path.GetFileNameWithoutExtension(caller?.GetFileName()) + ":" + caller?.GetFileLineNumber() + "]</color> " + message;
#endif
            return message;
        }

        /// <summary>
        /// Log a formatted line. Stacktrace is removed for LogType.Log out of editor unless the preprocessor OPENGET_DEBUG is defined.
        /// </summary>
        private static void LogLine(string formatted, LogType logType = LogType.Log, Object context = null) {
            try {
                UnityEngine.Debug.LogFormat(
                    logType,
#if UNITY_EDITOR || OPENGET_DEBUG
                    LogOption.None,
#else
                    logType != LogType.Log ? LogOption.None : LogOption.NoStacktrace,
#endif
                    context,
                    formatted
                );
            } catch (System.Exception e) {
                Log.Warning("Bad string format for logging! Exception occurred: " + e.ToString());
            }
        }

        /// <summary>
        /// Log for debugging purposes only. Calls not compiled outside the editor unless the OPENGET_DEBUG preprocessor is defined.
        /// If need be, you can use Log.Verbose() for detailed information but you should still avoid spamming.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("OPENGET_DEBUG")]
        public static void Debug(string message, params object[] args) {
            message = "Debug: " + message;
            unity.OnLog(Level.Debug, message, args);
            UpdateLoggers(Level.Debug, message, args);
        }

        /// <summary>
        /// Log some general info. This should be used as relatively significant markers of things that are expected to or has happened,
        /// e.g. starting and completing a scene load transition.
        /// </summary>
        public static void Info(string message, params object[] args) {
            message = "Info: " + message;
            unity.OnLog(Level.Info, message, args);
            UpdateLoggers(Level.Info, message, args);
        }

        /// <summary>
        /// Log a warning i.e. something that probably shouldn't happen but is accounted for. This should be used only sparingly.
        /// This logs to the standard info/debug log console. This is done because Unity outputs it's own warnings in the warnings console,
        /// which we don't want to confuse.
        /// </summary>
        public static void Warning(string message, params object[] args) {
            message = "Warning: " + message;
            unity.OnLog(Level.Warning, message, args);
            UpdateLoggers(Level.Warning, message, args);
        }

        /// <summary>
        /// Log an error i.e. something which should not happen and cannot be accounted for.
        /// </summary>
        public static void Error(string message, params object[] args) {
            message = "Error: " + message;
            unity.OnLog(Level.Error, message, args);
            UpdateLoggers(Level.Error, message, args);
        }

        /// <summary>
        /// Use this sparingly. Useful for logging technical bits of information about processes in releases.
        /// </summary>
        public static void Verbose(string message, params object[] args) {
            message = "Verbose: " + message;
            unity.OnLog(Level.Verbose, message, args);
            UpdateLoggers(Level.Verbose, message, args);
        }

        /// <summary>
        /// Log exceptions as errors without any other information.
        /// </summary>
        public static void Exception(System.Exception e) {
            string message = "Error: " + e.ToString();
            unity.OnLog(Level.Error, message);
            UpdateLoggers(Level.Error, message);
        }

    }

}
