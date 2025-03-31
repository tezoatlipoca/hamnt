
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


    public static class GlobalStatic
    {
        public static LogLevel CURRENT_LEVEL = LogLevel.Trace;
        public static string? CONFIG_FILE_PATH = null;
        public static string? NOTE_FILE_PATH = null;

        public static Dictionary<string, string> NOTE_FILES = new Dictionary<string, string>();

        public static string bldVersion = null;
        
        public static string NOTES_LOCATION = null;



        // define a constructor
        static GlobalStatic()
        {
            // set the default log level to Trace
            CURRENT_LEVEL = LogLevel.Trace;
            // determine the user's home directory
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // the CONFIG_FILE is user's home directory
            // + "/.config/hamnt/hamnt_config.json"
            NOTE_FILE_PATH = Path.Combine(homeDir, ".config", "hamnt", "hamnt.notefiles.json");
            CONFIG_FILE_PATH = Path.Combine(homeDir, ".config", "hamnt", "hamnt.config.json");
            // check if the file exists
            if (!File.Exists(NOTE_FILE_PATH))
            {
                // create the directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(NOTE_FILE_PATH));
                DBg.d(LogLevel.Trace, "Creating config file directory: " + NOTE_FILE_PATH);
                // create the file
                File.Create(NOTE_FILE_PATH).Close();
                DBg.d(LogLevel.Trace, "Creating config file: " + NOTE_FILE_PATH);
            }
            // read the note files from the config file
            ReadNoteFiles();
            if (!File.Exists(CONFIG_FILE_PATH))
            {
                // create the directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE_PATH));
                DBg.d(LogLevel.Trace, "Creating config file directory: " + CONFIG_FILE_PATH);
                // create the file
                File.Create(CONFIG_FILE_PATH).Close();
                DBg.d(LogLevel.Trace, "Creating config file: " + CONFIG_FILE_PATH);
            }
            ReadConfigFile();

            // lastly get the AssemblyInformationalVersion attribute from the assembly and store it in a static variable
            var bldVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            // convert it to a string and store it in a static variable
            bldVersion = bldVersionAttribute?.InformationalVersion;

        }

        public static void SetLogLevel(LogLevel level)
        {
            CURRENT_LEVEL = level;
        }

        public static void ReadNoteFiles()
        {
            // read the note files from the config file
            if (File.Exists(NOTE_FILE_PATH))
            {
                string json = File.ReadAllText(NOTE_FILE_PATH);
                try
                {
                    NOTE_FILES = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    DBg.d(LogLevel.Error, "BAD NOTES FILE: " + ex.Message);
                    NOTE_FILES = new Dictionary<string, string>();
                }
            }
            else
            {
                DBg.d(LogLevel.Trace, "Note Files file not found: " + NOTE_FILE_PATH);
            }
        }
        public static void WriteNoteFiles()
        {
            // write the note files to the config file
            if (NOTE_FILES.Count > 0)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(NOTE_FILES);
                DBg.d(LogLevel.Trace, "JSON:" + json);
                File.WriteAllText(NOTE_FILE_PATH, json);
                DBg.d(LogLevel.Trace, "Writing note files to: " + NOTE_FILE_PATH);
            }
            else
            {
                DBg.d(LogLevel.Trace, "No note files to write to: " + NOTE_FILE_PATH);
                // write an empty file
                File.WriteAllText(NOTE_FILE_PATH, "{}");
            }
        }
        public static void ReadConfigFile()
        {
            // read the config file
            if (File.Exists(CONFIG_FILE_PATH))
            {
                string json = File.ReadAllText(CONFIG_FILE_PATH);
                try
                {
                    var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (config != null && config.ContainsKey("NOTES_LOCATION"))
                    {
                        NOTES_LOCATION = config["NOTES_LOCATION"];
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    DBg.d(LogLevel.Error, "BAD CONFIG FILE: " + ex.Message);
                }
            }
            else
            {
                DBg.d(LogLevel.Trace, "Config file not found: " + CONFIG_FILE_PATH);
            }
        }
        public static void WriteConfigFile()
        {
            // write the config file
            if (NOTES_LOCATION != null)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "NOTES_LOCATION", NOTES_LOCATION } });
                DBg.d(LogLevel.Trace, "JSON:" + json);
                File.WriteAllText(CONFIG_FILE_PATH, json);
                DBg.d(LogLevel.Trace, "Writing config file: " + CONFIG_FILE_PATH);
            }
            else
            {
                DBg.d(LogLevel.Trace, "No config file to write: " + CONFIG_FILE_PATH);
                // write an empty file
                File.WriteAllText(CONFIG_FILE_PATH, "{}");
            }
        }
    }
