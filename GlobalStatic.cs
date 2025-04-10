using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class JsonContext : JsonSerializerContext { }

public static class GlobalStatic
{
    public static string? CONFIG_FILE_PATH = null;
    public static string? NOTE_FILE_PATH = null;

    public static Dictionary<string, string> NOTE_FILES = new Dictionary<string, string>();

    public static string bldVersion = null;
    
    public static Dictionary<string, string> PARAMETERS = new Dictionary<string, string>();


    public static bool interactiveMode = true;

    // define a constructor
    static GlobalStatic()
    {
        
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

        // now set default PARAMETERS values but ONLY if they weren't in 
        // the config file we just read in.
        if (!PARAMETERS.ContainsKey("NOTES_LOCATION"))
        {
            PARAMETERS["NOTES_LOCATION"] = Path.Combine(homeDir, ".config", "hamnt", "notes");
        }
        if (!PARAMETERS.ContainsKey("LOG_LEVEL"))
        {
            PARAMETERS["LOG_LEVEL"] = LogLevel.Trace.ToString();
        }

        

    }

    public static void SetLogLevel(LogLevel level)
    {
        // get the old value first and log the change debug msg at the old level
        LogLevel oldLevel = LogLevel.Trace;
        if (PARAMETERS["LOG_LEVEL"] is not null)
        {
            LogLevel.TryParse(PARAMETERS["LOG_LEVEL"], out oldLevel);
            DBg.d(oldLevel, "Changing log level from " + oldLevel.ToString() + " to " + level.ToString());
        }
        PARAMETERS["LOG_LEVEL"] = level.ToString();
    }

    public static void ReadNoteFiles()
    {
        // read the note files from the config file
        if (File.Exists(NOTE_FILE_PATH))
        {
            string json = File.ReadAllText(NOTE_FILE_PATH);
            try
            {
                var options = new JsonSerializerOptions { TypeInfoResolver = JsonContext.Default };
                NOTE_FILES = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
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
            var options = new JsonSerializerOptions 
            { 
                TypeInfoResolver = JsonContext.Default,
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(NOTE_FILES, options);
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
        if (File.Exists(CONFIG_FILE_PATH))
        {
            try
            {
                string json = File.ReadAllText(CONFIG_FILE_PATH);
                var options = new JsonSerializerOptions { TypeInfoResolver = JsonContext.Default };
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
                
                if (config != null)
                {
                    // Only update PARAMETERS if the key exists in config
                    if (config.ContainsKey("NOTES_LOCATION"))
                    {
                        PARAMETERS["NOTES_LOCATION"] = config["NOTES_LOCATION"];
                    }
                    if (config.ContainsKey("LOG_LEVEL"))
                    {
                        PARAMETERS["LOG_LEVEL"] = config["LOG_LEVEL"];
                    }
                }
            }
            catch (JsonException ex)
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
        
            string json = System.Text.Json.JsonSerializer.Serialize(PARAMETERS);
            DBg.d(LogLevel.Trace, "JSON:" + json);
            File.WriteAllText(CONFIG_FILE_PATH, json);
            DBg.d(LogLevel.Trace, "Writing config file: " + CONFIG_FILE_PATH);
        
        
    }
}
