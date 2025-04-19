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
        string homeDir;

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            // We're running as a snap
            homeDir = Environment.GetEnvironmentVariable("SNAP_USER_COMMON")!;
            // For snap, we'll store everything directly in SNAP_USER_COMMON
            NOTE_FILE_PATH = Path.Combine(homeDir, "hamnt.notefiles.json");
            CONFIG_FILE_PATH = Path.Combine(homeDir, "hamnt.config.json");
        }
        else if (OperatingSystem.IsWindows())
        {
            // On Windows, use AppData/Local
            homeDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            NOTE_FILE_PATH = Path.Combine(homeDir, "hamnt", "hamnt.notefiles.json");
            CONFIG_FILE_PATH = Path.Combine(homeDir, "hamnt", "hamnt.config.json");

            // Ensure the directory exists
            Directory.CreateDirectory(Path.Combine(homeDir, "hamnt"));
        }
        else
        {
            // Standard Linux/macOS installation
            homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // the CONFIG_FILE is user's home directory + "/.config/hamnt/hamnt_config.json"
            NOTE_FILE_PATH = Path.Combine(homeDir, ".config", "hamnt", "hamnt.notefiles.json");
            CONFIG_FILE_PATH = Path.Combine(homeDir, ".config", "hamnt", "hamnt.config.json");
        }
        // check if the file exists
        if (!File.Exists(CONFIG_FILE_PATH))
        {
            // create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE_PATH!)!);
            DBg.d(LogLevel.Warning, "Creating config file directory: " + CONFIG_FILE_PATH);
            // create the file
            var defaultConfig = new Dictionary<string, string>
            {
                { "LOG_LEVEL", LogLevel.Information.ToString() }
            };
            string json = JsonSerializer.Serialize(defaultConfig, JsonContext.Default.DictionaryStringString);
            File.WriteAllText(CONFIG_FILE_PATH, json);

            DBg.d(LogLevel.Warning, "Creating config file: " + CONFIG_FILE_PATH);
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
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP_USER_COMMON")))
            {
                PARAMETERS["NOTES_LOCATION"] = Path.Combine(
                    Environment.GetEnvironmentVariable("SNAP_USER_COMMON")!, "notes");
            }
            else
            {
                // if in Windows, use AppData/Local
                if (OperatingSystem.IsWindows())
                {
                    PARAMETERS["NOTES_LOCATION"] = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "hamnt", "notes");
                }
                else
                {
                    PARAMETERS["NOTES_LOCATION"] = Path.Combine(homeDir, ".config", "hamnt", "notes");
                }
            }
        }
        if (!PARAMETERS.ContainsKey("CASE_SENSITIVE"))
        {
            PARAMETERS["CASE_SENSITIVE"] = "false";
        }
        if (!PARAMETERS.ContainsKey("MATCH_MODE"))
        {
            PARAMETERS["MATCH_MODE"] = "Contains";
        }
        




        if (!File.Exists(NOTE_FILE_PATH))
        {
            // create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(NOTE_FILE_PATH!)!);
            DBg.d(LogLevel.Trace, "Creating config directory: " + NOTE_FILE_PATH);
            // create the file
            File.WriteAllText(NOTE_FILE_PATH, "{}");
            DBg.d(LogLevel.Trace, "Creating notes file: " + NOTE_FILE_PATH);
        }
        // read the note files from the config file
        ReadNoteFiles();


        // lastly get the AssemblyInformationalVersion attribute from the assembly and store it in a static variable
        var bldVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        // convert it to a string and store it in a static variable
        if (bldVersionAttribute?.InformationalVersion != null)
        {
            string fullVersion = bldVersionAttribute.InformationalVersion;
            
            // Check if the version contains a '+' which separates version from git hash
            int plusIndex = fullVersion.IndexOf('+');
            if (plusIndex >= 0 && plusIndex < fullVersion.Length - 1)
            {
                // Extract the base version and git hash
                string baseVersion = fullVersion.Substring(0, plusIndex);
                string gitHash = fullVersion.Substring(plusIndex + 1);
                
                // Truncate git hash to 7 characters if it's longer
                if (gitHash.Length > 7)
                {
                    gitHash = gitHash.Substring(0, 7);
                }
                
                // Combine the base version with the truncated git hash
                bldVersion = $"{baseVersion}+{gitHash}";
            }
            else
            {
                // If there's no git hash or the format is different, use the full version
                bldVersion = fullVersion;
            }
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
