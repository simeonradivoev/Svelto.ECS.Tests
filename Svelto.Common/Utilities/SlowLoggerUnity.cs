#if UNITY_5_3_OR_NEWER || UNITY_5
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Svelto.Utilities
{
    public class SlowUnityLogger : ILogger
    {
        public SlowUnityLogger()
        {
            StringBuilder ValueFactory() => new StringBuilder();

            _stringBuilder = new ThreadLocal<StringBuilder>(ValueFactory);
        }

        public void Log(string txt, LogType type = LogType.Log, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            switch (type)
            {
                case LogType.Log:
                    Debug.Log(txt);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(txt);
                    break;
                case LogType.Error:
                case LogType.Exception:
                    string stack;
                    txt = txt.FastConcat(e.Message);
                    stack = ExtractFormattedStackTrace(new StackTrace(e, true));

                    Debug.LogError("<color=orange> ".FastConcat(txt, "</color> ", Environment.NewLine, stack)
                        .FastConcat(Environment.NewLine, dataString));
                    break;
            }
        }

        public void OnLoggerAdded()
        {
            projectFolder = Application.dataPath.Replace("Assets", "");

            Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Exception, StackTraceLogType.None);
#if !UNITY_EDITOR || PROFILER
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
#endif
            Console.Log("Slow Unity Logger added");
        }

        /// <summary>
        ///     copied from Unity source code, whatever....
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            _stringBuilder.Value.Length = 0;

            var frame = new StackTrace(true);

            for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
            {
                FormatStack(stackTrace, index1, _stringBuilder.Value);
            }

            for (var index1 = 4; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, _stringBuilder.Value);
            }

            return _stringBuilder.ToString();
        }

        void FormatStack(StackTrace stackTrace, int index1, StringBuilder stringBuilder)
        {
            var frame = stackTrace.GetFrame(index1);
            var method = frame.GetMethod();
            if (method != null)
            {
                var declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    var str1 = declaringType.Namespace;
                    if (!string.IsNullOrEmpty(str1))
                    {
                        stringBuilder.Append(str1);
                        stringBuilder.Append(".");
                    }

                    stringBuilder.Append(declaringType.Name);
                    stringBuilder.Append(":");
                    stringBuilder.Append(method.Name);
                    stringBuilder.Append("(");
                    var index2 = 0;
                    var parameters = method.GetParameters();
                    var flag = true;
                    for (; index2 < parameters.Length; ++index2)
                    {
                        if (!flag)
                            stringBuilder.Append(", ");
                        else
                            flag = false;
                        stringBuilder.Append(parameters[index2].ParameterType.Name);
                    }

                    stringBuilder.Append(")");
                    var str2 = frame.GetFileName();
                    if (str2 != null &&
                        (!(declaringType.Name == "Debug") || !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "Logger") || !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "DebugLogHandler") ||
                         !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "Assert") ||
                         !(declaringType.Namespace == "UnityEngine.Assertions")) && (!(method.Name == "print") ||
                                                                                     !(declaringType.Name ==
                                                                                       "MonoBehaviour") ||
                                                                                     !(declaringType.Namespace ==
                                                                                       "UnityEngine")))
                    {
                        stringBuilder.Append(" (at ");
#if UNITY_EDITOR
                        str2 = str2.Replace(@"\", "/");
                        if (!string.IsNullOrEmpty(projectFolder) && str2.StartsWith(projectFolder))

                            str2 = str2.Substring(projectFolder.Length, str2.Length - projectFolder.Length);
#endif
                        stringBuilder.Append(str2);
                        stringBuilder.Append(":");
                        stringBuilder.Append(frame.GetFileLineNumber().ToString());
                        stringBuilder.Append(")");
                    }

                    stringBuilder.Append("\n");
                }
            }
        }

        readonly ThreadLocal<StringBuilder> _stringBuilder;

        static string projectFolder;
    }
}
#endif