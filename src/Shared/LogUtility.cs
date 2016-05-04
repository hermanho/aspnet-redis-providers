//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Globalization;
using System.IO;

namespace Microsoft.Web.Redis
{
    internal static class LogUtility
    {
        private static readonly System.Diagnostics.TraceSource trace = new System.Diagnostics.TraceSource("Microsoft.Web.Redis");
        public static TextWriter logger = null;

        public static void LogWarning(string msg, params object[] args)
        {
            trace.TraceEvent(System.Diagnostics.TraceEventType.Warning, 1, msg, args);
            Log("[Warning]", msg, args);
        }

        public static void LogError(string msg, params object[] args)
        {
            trace.TraceEvent(System.Diagnostics.TraceEventType.Error, 2, msg, args);
            Log("[Error]", msg, args);
        }

        public static void LogInfo(string msg, params object[] args)
        {
            trace.TraceEvent(System.Diagnostics.TraceEventType.Information, 3, msg, args);
            Log("[Info]", msg, args);
        }

        private static void Log(string type, string msg, params object[] args)
        {
            if (logger != null)
            {
                string msgToPrint = (args.Length > 0) ? string.Format(msg, args) : msg;
                logger.WriteLine("[{0}]{1}{2}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), type, msgToPrint);
            }
        }
    } 
}
