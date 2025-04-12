using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System.IO;
using System;




    public static class DBg
    {

        public static void d(LogLevel level,
                string? msg,
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0,
                [CallerMemberName] string member = "")

        {
            // get the current date and time in ISO 6801 format:
            // YYYY-MM-DD HH:MM:SS
            string now = DateTime.Now.ToString("s");
            var debugNfo = "";
            if (file is not null && member is not null)
            {
                // trick won't work if we run on a diff platform than we were compiled on:
                string normalizedFile = file.Replace('/', Path.DirectorySeparatorChar)
                                .Replace('\\', Path.DirectorySeparatorChar);

                string filename = Path.GetFileName(normalizedFile);
                debugNfo = $"[{member}//{filename}:{line}]";
            }


            // try and cast GlobalStatic.PARAMETERS["LOG_LEVEL"]
            // back to a LogLevel; if it fails, default to Trace
            LogLevel gsCurrLevel = LogLevel.Trace;
            if (GlobalStatic.PARAMETERS is not null && GlobalStatic.PARAMETERS.ContainsKey("LOG_LEVEL") && GlobalStatic.PARAMETERS["LOG_LEVEL"] is not null) {
                try
                {
                    gsCurrLevel = (LogLevel)Enum.Parse(typeof(LogLevel), GlobalStatic.PARAMETERS["LOG_LEVEL"]);
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine($"db.dump | FATAL: {e.Message}");
                    level = LogLevel.Trace;
                }
            }           
            if (level < gsCurrLevel)
            {
                return;
            }
            switch (level)
            {
                case LogLevel.Trace:
                    if (debugNfo is not null)
                    {
                        Console.WriteLine($"{now} TRACE | {debugNfo} {msg}");
                    }
                    else
                    {
                        Console.WriteLine($"{now} TRACE | {msg}");
                    }
                    return;
                case LogLevel.Debug:
                    if (debugNfo is not null)
                    {
                        Console.WriteLine($"{now} DEBUG | {debugNfo} {msg}");
                    }
                    else
                    {
                        Console.WriteLine($"{now} DEBUG | {msg}");
                    }

                    return;
                case LogLevel.Information:
                    Console.WriteLine($"{now} INFO  | {msg}");
                    return;
                case LogLevel.Warning:
                    Console.WriteLine($"{now} WARN  | {msg}");
                    return;
                case LogLevel.Error:
                    Console.WriteLine($"{now} ERROR | {msg}");
                    return;
                case LogLevel.Critical:
                    Console.WriteLine($"{now} FATAL | {msg}");
                    return;
                default:
                    Console.WriteLine($"db.dump | FATAL: Unexpected value for level: {msg}");
                    return;
            }
        }
    }
