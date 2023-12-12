using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        /// Log priority levels.
        /// </summary>
        [System.Flags]
        public enum Level {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 4,
            Verbose = 8,
            All = 15
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
            foreach (KeyValuePair<ILogger, Level> logger in loggers) {
                if ((logger.Value | level) != 0) {
                    logger.Key.OnLog(level, message, args);
                }
            }
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
        private static string PrefixStackInfo(string message) {
#if UNITY_EDITOR
            System.Diagnostics.StackFrame caller = new System.Diagnostics.StackTrace(true).GetFrame(2);
            message = "<color=#AFA>[" + 
                System.IO.Path.GetFileNameWithoutExtension(caller.GetFileName()) + ":" + caller.GetFileLineNumber() + "]</color> " + message;
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
            LogLine(PrefixStackInfo(Format("cyan", message, args)));
            UpdateLoggers(Level.Debug, message, args);
        }

        /// <summary>
        /// Log some general info. This should be used as relatively significant markers of things that are expected to or has happened,
        /// e.g. starting and completing a scene load transition.
        /// </summary>
        public static void Info(string message, params object[] args) {
            message = "Info: " + message;
            LogLine(PrefixStackInfo(Format("white", message, args)));
            UpdateLoggers(Level.Info, message, args);
        }

        /// <summary>
        /// Log a warning i.e. something that probably shouldn't happen but is accounted for. This should be used only sparingly.
        /// This logs to the standard info/debug log console. This is done because Unity outputs it's own warnings in the warnings console,
        /// which we don't want to confuse.
        /// </summary>
        public static void Warning(string message, params object[] args) {
            message = "Warning: " + message;
            LogLine(PrefixStackInfo(Format("yellow", message, args)));
            UpdateLoggers(Level.Warning, message, args);
        }

        /// <summary>
        /// Log an error i.e. something which should not happen and cannot be accounted for.
        /// </summary>
        public static void Error(string message, params object[] args) {
            message = "Error: " + message;
#if UNITY_EDITOR
            LogLine(PrefixStackInfo(Format("red", message, args)));
#endif
            // Also log to the regular error console
            LogLine(PrefixStackInfo(Format(null, message, args)), LogType.Error);
            UpdateLoggers(Level.Error, message, args);
        }

        /// <summary>
        /// Use this sparingly. Useful for logging technical bits of information about processes in releases.
        /// </summary>
        public static void Verbose(string message, params object[] args) {
            message = "Verbose: " + message;
            LogLine(PrefixStackInfo(Format("green", message, args)));
            UpdateLoggers(Level.Verbose, message, args);
        }

        /// <summary>
        /// Log exceptions as errors without any other information.
        /// </summary>
        public static void Exception(System.Exception e) {
            string message = "Error: " + e.ToString();
#if UNITY_EDITOR
            LogLine(PrefixStackInfo(Format("red", message)));
#endif
            // Log with the exception log type
            LogLine(PrefixStackInfo(Format(null, message)), LogType.Exception);
            UpdateLoggers(Level.Error, message);
        }

    }

}
