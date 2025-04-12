using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class JsonContext : JsonSerializerContext { }

public static class GlobalStatic
{
    public static string? CONFIG_FILE_PATH = null;
    public static string? NOTE_FILE_PATH = null;
    public static string? bldVersion = null;
    public static Dictionary<string, string> NOTE_FILES = new();
    public static Dictionary<string, string> PARAMETERS = new();
    public static bool interactiveMode = true;

    public static bool noteFilesChanged = false; // taint flag; if changed, save before exit.
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
        if (!File.Exists(CONFIG_FILE_PATH))
        {
            // create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE_PATH!)!);
            DBg.d(LogLevel.Trace, "Creating config file directory: " + CONFIG_FILE_PATH);
            // create the file
            File.Create(CONFIG_FILE_PATH).Close();
            DBg.d(LogLevel.Trace, "Creating config file: " + CONFIG_FILE_PATH);
        }
        // set the default log level to Information
        if (!PARAMETERS.ContainsKey("LOG_LEVEL"))
        {
            PARAMETERS["LOG_LEVEL"] = LogLevel.Information.ToString();
        }
        
        // which means we never get Trace or Debug in .ReadConfigFile, but whatever. 
        ReadConfigFile();
        // now set default PARAMETERS values but ONLY if they weren't in 
        // the config file we just read in.
        if (!PARAMETERS.ContainsKey("NOTES_LOCATION"))
        {
            PARAMETERS["NOTES_LOCATION"] = Path.Combine(homeDir, ".config", "hamnt", "notes");
        }
        
        
        
        
        if (!File.Exists(NOTE_FILE_PATH))
        {
            // create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(NOTE_FILE_PATH!)!);
            DBg.d(LogLevel.Trace, "Creating config file directory: " + NOTE_FILE_PATH);
            // create the file
            File.Create(NOTE_FILE_PATH).Close();
            DBg.d(LogLevel.Trace, "Creating config file: " + NOTE_FILE_PATH);
        }
        // read the note files from the config file
        ReadNoteFiles();
        

        // lastly get the AssemblyInformationalVersion attribute from the assembly and store it in a static variable
        var bldVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        // convert it to a string and store it in a static variable
        bldVersion = bldVersionAttribute?.InformationalVersion;

        



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
        if (File.Exists(NOTE_FILE_PATH))
        {
            string json = File.ReadAllText(NOTE_FILE_PATH);
            try
            {
                NOTE_FILES = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringString) 
                    ?? new Dictionary<string, string>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                DBg.d(LogLevel.Error, "BAD NOTES FILE: " + ex.Message);
                NOTE_FILES = new Dictionary<string, string>();
            }
        }
    }
    public static void WriteNoteFiles()
    {
        if (NOTE_FILES.Count > 0)
        {
            string json = JsonSerializer.Serialize(NOTE_FILES, JsonContext.Default.DictionaryStringString);
            DBg.d(LogLevel.Trace, "JSON:" + json);
            File.WriteAllText(NOTE_FILE_PATH!, json);
            DBg.d(LogLevel.Trace, "Writing note files to: " + NOTE_FILE_PATH);
        }
        else
        {
            DBg.d(LogLevel.Trace, "No note files to write to: " + NOTE_FILE_PATH);
            File.WriteAllText(NOTE_FILE_PATH!, "{}");
        }
    }
    public static void ReadConfigFile()
    {
        if (File.Exists(CONFIG_FILE_PATH))
        {
            try
            {
                string json = File.ReadAllText(CONFIG_FILE_PATH);
                DBg.d(LogLevel.Trace, "JSON:" + json);
                var config = JsonSerializer.Deserialize(json, JsonContext.Default.DictionaryStringString);
                if (config != null)
                {
                    PARAMETERS = config;
                }
            }
            catch (JsonException ex)
            {
                DBg.d(LogLevel.Error, "BAD CONFIG FILE: " + ex.Message);
            }
        }
    }
    public static void WriteConfigFile()
    {
        string json = JsonSerializer.Serialize(PARAMETERS, JsonContext.Default.DictionaryStringString);
        DBg.d(LogLevel.Trace, "JSON:" + json);
        File.WriteAllText(CONFIG_FILE_PATH!, json);
        DBg.d(LogLevel.Trace, "Writing config file: " + CONFIG_FILE_PATH);
    }
}
