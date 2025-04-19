using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;


namespace hamnt
{
    public class hamntEngine
    {

        public void AddNoteFile(string[] tokens)
        {
            var alias = string.Empty;
            var filePath = string.Empty;
            if (GlobalStatic.interactiveMode)
            {
                Console.WriteLine("Enter alias: ");
                alias = Console.ReadLine();
                if (string.IsNullOrEmpty(alias))
                {
                    DBg.d(LogLevel.Error, "Error: No list alias specified.");
                    return;
                }
                if (!string.IsNullOrEmpty(GlobalStatic.PARAMETERS["NOTES_LOCATION"]))
                {
                    Console.WriteLine($"Enter filename (in {GlobalStatic.PARAMETERS["NOTES_LOCATION"]}) or absolute: ");
                }
                else
                {
                    Console.WriteLine("Enter absolute filename or file in current directory.");
                }
                filePath = Console.ReadLine();
                if (string.IsNullOrEmpty(filePath))
                {
                    DBg.d(LogLevel.Error, "No note filename/path specified.");
                    return;
                }
            }
            else
            {
                // alias is the 2nd token in tokens array
                if (tokens.Length < 3)
                {
                    DBg.d(LogLevel.Error, "No note file alias or filename specified.");
                    return;
                }
                alias = tokens[1];
                filePath = tokens[2];
            }
            // if the alias is already in the dictionary, remind the user which file it is
            if (GlobalStatic.NOTE_FILES.ContainsKey(alias))
            {
                DBg.d(LogLevel.Warning, $"Alias '{alias}' already exists: {GlobalStatic.NOTE_FILES[alias]}");
                DBg.d(LogLevel.Warning, $"If you want to redefine it, please remove '{alias}' first.");
                return;
            }

            // determine if the file path is an absolute or relative path
            // if it's relative, make it absolute
            if (!Path.IsPathRooted(filePath))
            {
                // make it absolute; if we have a configured NOTES_LOCATION, use that
                // otherwise, use the current directory
                if (GlobalStatic.PARAMETERS["NOTES_LOCATION"] != null)
                {
                    filePath = Path.Combine(GlobalStatic.PARAMETERS["NOTES_LOCATION"], filePath);
                }
                else
                {
                    // use the current directory
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                }
            }
            GlobalStatic.NOTE_FILES[alias] = filePath;
            DBg.d(LogLevel.Information, $"Added note file: {alias} -> {filePath}");
            GlobalStatic.noteFilesChanged = true;

            if (!File.Exists(filePath))
            {
                // create the file
                DBg.d(LogLevel.Warning, $"File does not exist but will if something is added to it: {filePath}");
            }
        }
        public void RemoveNoteFile(string[] tokens)
        {
            var alias = string.Empty;
            if (GlobalStatic.interactiveMode)
            {
                Console.WriteLine("Enter alias: ");
                alias = Console.ReadLine();
                if (string.IsNullOrEmpty(alias))
                {
                    DBg.d(LogLevel.Error, "No note file alias specified.");
                    return;
                }
                else if (!GlobalStatic.NOTE_FILES.ContainsKey(alias))

                {
                    DBg.d(LogLevel.Warning, $"Alias {alias} not found.");
                    return;
                }
            }
            else
            {
                // alias is the 2nd token in tokens array
                if (tokens.Length < 2)
                {
                    DBg.d(LogLevel.Error, "No note file alias to delete specified.");
                    return;
                }
                alias = tokens[1];
            }

            if (GlobalStatic.NOTE_FILES.ContainsKey(alias))
            {
                GlobalStatic.NOTE_FILES.Remove(alias);
                DBg.d(LogLevel.Information, $"Removed note file: {alias}");
                GlobalStatic.noteFilesChanged = true;
            }
            else
            {
                DBg.d(LogLevel.Warning, $"Alias {alias} not found.");
            }
        }


        public void ListNoteFiles()
        {
            DBg.d(LogLevel.Trace, "Listing note files");
            foreach (var noteFile in GlobalStatic.NOTE_FILES)
            {
                Console.WriteLine($"{noteFile.Key} -> {noteFile.Value}");
            }
        }
        public void SetParameter(string[] tokens)
        {
            bool didAnytingChange = false;
            var paramName = string.Empty;
            var paramValue = string.Empty;
            var originalParamValue = string.Empty;
            // if we get here via cli, the parameter name will be 2nd param, value 3rd
            // if we get here via interactive mode we can prompt
            if (GlobalStatic.interactiveMode)
            {
                Console.WriteLine("Enter parameter name: ");
                paramName = Console.ReadLine();
                if (string.IsNullOrEmpty(paramName))
                {
                    DBg.d(LogLevel.Error, "Parameter name is null or empty.");
                    return;
                }
                var validParams = new HashSet<string> { "NOTES_LOCATION", "LOG_LEVEL", "CASE_SENSITIVE", "MATCH_MODE" };
                if (!validParams.Contains(paramName))
                {
                    DBg.d(LogLevel.Error, $"Parameter '{paramName}' not valid.");
                    return;
                }
                Console.WriteLine("Enter parameter value: ");
                paramValue = Console.ReadLine();
                if (string.IsNullOrEmpty(paramValue))
                {
                    DBg.d(LogLevel.Error, "Parameter value is null or empty.");
                    return;
                }
            }
            else
            {

                if (tokens.Length < 3)
                {
                    DBg.d(LogLevel.Error, "Not enough parameters. Try `hamnt --set <parameter> <value>`");
                    DBg.d(LogLevel.Error, "           parameters: NOTES_LOCATION, LOG_LEVEL, CASE_SENSITIVE");
                    return;
                }
                paramName = tokens[1];
                paramValue = tokens[2];
            }
            // the above can't handle cases where parameter names NOR parameter values are quoted/include spaces.

            // check to see if the parameter name is not null/empty
            if (string.IsNullOrEmpty(paramName))
            {
                DBg.d(LogLevel.Error, "Parameter name is null or empty.");
                return;
            }

            switch (paramName.ToUpper())
            {
                case "NOTES_LOCATION":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        DBg.d(LogLevel.Error, "No NOTES_LOCATION provided.");
                        return;
                    }
                    else
                    {
                        // check to see if the parameter value is a valid path
                        if (!Path.IsPathRooted(paramValue))
                        {
                            DBg.d(LogLevel.Warning, $"NOTES_LOCATION '{paramValue}' is not a valid path; will be created if needed");

                        }

                        // check to see if the parameter value is a directory
                        if (!Directory.Exists(paramValue))
                        {
                            DBg.d(LogLevel.Warning, $"NOTES_LOCATION '{paramValue}' is not a directory.");

                        }
                        originalParamValue = GlobalStatic.PARAMETERS["NOTES_LOCATION"];
                        GlobalStatic.PARAMETERS["NOTES_LOCATION"] = paramValue;
                        if (originalParamValue != paramValue)
                        {

                            didAnytingChange = true;
                            DBg.d(LogLevel.Trace, $"Set NOTES_LOCATION: {paramValue}");
                        }
                        else
                        {
                            DBg.d(LogLevel.Warning, $"NOTES_LOCATION already set to {paramValue}");
                        }
                    }
                    break;
                case "LOG_LEVEL":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        DBg.d(LogLevel.Error, "LOG_LEVEL is null or empty.");
                        return;
                    }
                    // check to see if the parameter value is a valid LogLevel
                    if (!Enum.TryParse(typeof(LogLevel), paramValue, true, out _))
                    {
                        DBg.d(LogLevel.Error, $"LOG_LEVEL '{paramValue}' is not valid. Try one of: {string.Join(", ", Enum.GetNames(typeof(LogLevel)))}.");
                        return;
                    }
                    else
                    {
                        // set the parameter value.. if it CHANGED value that is...
                        originalParamValue = GlobalStatic.PARAMETERS["LOG_LEVEL"];
                        if (originalParamValue != paramValue)
                        {
                            GlobalStatic.SetLogLevel((LogLevel)Enum.Parse(typeof(LogLevel), paramValue));
                            didAnytingChange = true;
                            DBg.d(LogLevel.Trace, $"Set LOG_LEVEL: {paramValue}");
                        }
                        else
                        {
                            DBg.d(LogLevel.Warning, $"LOG_LEVEL already set to {paramValue}");
                        }

                    }
                    break;
                case "CASE_SENSITIVE":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        DBg.d(LogLevel.Error, "CASE_SENSITIVE is null or empty.");
                        return;
                    }
                    // check to see if the parameter value is a valid LogLevel
                    if (!bool.TryParse(paramValue, out _))
                    {
                        DBg.d(LogLevel.Error, $"CASE_SENSITIVE '{paramValue}' is not valid. Try one of: true, false.");
                        return;
                    }
                    else
                    {
                        // set the parameter value.. if it CHANGED value that is...
                        originalParamValue = GlobalStatic.PARAMETERS["CASE_SENSITIVE"];
                        if (originalParamValue != paramValue)
                        {
                            GlobalStatic.PARAMETERS["CASE_SENSITIVE"] = paramValue;
                            didAnytingChange = true;
                            DBg.d(LogLevel.Trace, $"Set CASE_SENSITIVE: {paramValue}");
                        }
                        else
                        {
                            DBg.d(LogLevel.Warning, $"CASE_SENSITIVE already set to {paramValue}");
                        }

                    }
                    break;
                case "MATCH_MODE":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        DBg.d(LogLevel.Error, "MATCH_MODE is null or empty.");
                        return;
                    }

                    // Check if it's a valid match mode
                    var validMatchModes = new[] { "Contains", "Exact", "Any" };
                    if (!validMatchModes.Contains(paramValue, StringComparer.OrdinalIgnoreCase))
                    {
                        DBg.d(LogLevel.Error, $"MATCH_MODE '{paramValue}' is not valid. Try one of: {string.Join(", ", validMatchModes)}.");
                        return;
                    }
                    else
                    {
                        // set the parameter value if it changed
                        originalParamValue = GlobalStatic.PARAMETERS["MATCH_MODE"];
                        if (originalParamValue != paramValue)
                        {
                            GlobalStatic.PARAMETERS["MATCH_MODE"] = paramValue;
                            didAnytingChange = true;
                            DBg.d(LogLevel.Trace, $"Set MATCH_MODE: {paramValue}");
                        }
                        else
                        {
                            DBg.d(LogLevel.Warning, $"MATCH_MODE already set to {paramValue}");
                        }
                    }
                    break;
                default:
                    // do nothing
                    DBg.d(LogLevel.Warning, $"Error: Parameter '{paramName}' not valid.");
                    break;
            }


            if (didAnytingChange)
            {
                GlobalStatic.WriteConfigFile();
            }
        }
        public void EditNoteFile(string[] tokens)
        {
            var alias = string.Empty;
            if (GlobalStatic.interactiveMode)
            {
                Console.WriteLine("Enter alias: ");
                alias = Console.ReadLine();
                if (string.IsNullOrEmpty(alias))
                {
                    DBg.d(LogLevel.Error, "No note file alias specified.");
                    return;
                }
            }
            else
            {
                // alias is the 2nd token in tokens array
                if (tokens.Length < 2)
                {
                    DBg.d(LogLevel.Error, "No note file alias specified.");
                    return;
                }
                alias = tokens[1];
            }

            if (GlobalStatic.NOTE_FILES.ContainsKey(alias))
            {
                var filePath = GlobalStatic.NOTE_FILES[alias];
                //filePath = $"\"{filePath}\"";
                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    DBg.d(LogLevel.Warning, $"File {filePath} does not exist.");
                    return;
                }
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        // Use Process.Start to open file with default application in Windows
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                        DBg.d(LogLevel.Information, $"Opened {alias} ({filePath}) with default application");
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        // For Linux, we need to consider terminal-based editors

                        // Check if EDITOR environment variable is set
                        string editor = Environment.GetEnvironmentVariable("EDITOR") ?? string.Empty;
                        if (string.IsNullOrEmpty(editor))
                        {
                            // Fallback to xdg-open if EDITOR is not set
                            editor = "xdg-open";
                            DBg.d(LogLevel.Warning, "env{EDITOR} not set (e.g. export EDITOR=vi), using xdg-open as fallback.");
                        }
                        // For any editor, use the current terminal by executing it directly
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = editor,
                                Arguments = $"\"{filePath}\"",
                                UseShellExecute = false
                            }
                        };

                        try
                        {
                            process.Start();
                            process.WaitForExit();
                            DBg.d(LogLevel.Information, $"Finished editing {alias} with {editor}");
                        }
                        catch (Exception ex)
                        {
                            // If the editor fails (not found, etc.), fall back to xdg-open
                            DBg.d(LogLevel.Warning, $"Failed to open with {editor}: {ex.Message}");
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "xdg-open",
                                Arguments = $"\"{filePath}\"",
                                UseShellExecute = true
                            });
                            DBg.d(LogLevel.Information, $"Opened {alias} ({filePath}) with default application");
                        }
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        // On macOS, use open
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "open",
                            Arguments = filePath,
                            UseShellExecute = true
                        });
                        DBg.d(LogLevel.Information, $"Opened {alias} ({filePath}) with default application");
                    }
                }
                catch (Exception ex)
                {
                    DBg.d(LogLevel.Error, $"Failed to open {alias} ({filePath}): {ex.Message}");
                }
            }
            else
            {
                DBg.d(LogLevel.Warning, $"Alias {alias} not found.");
            }
        }

        // public bool ChangeDirectory(string[] tokens)
        // {
        //     var alias = string.Empty;
        //     if (GlobalStatic.interactiveMode)
        //     {
        //         Console.WriteLine("Enter alias: ");
        //         alias = Console.ReadLine();
        //         if (string.IsNullOrEmpty(alias))
        //         {
        //             DBg.d(LogLevel.Error, "No note file alias specified.");
        //             return false;
        //         }
        //     }
        //     else
        //     {
        //         // alias is the 2nd token in tokens array
        //         if (tokens.Length < 2)
        //         {
        //             DBg.d(LogLevel.Error, "No note file alias specified.");
        //             return false; ;
        //         }
        //         alias = tokens[1];
        //     }

        //     if (GlobalStatic.NOTE_FILES.ContainsKey(alias))
        //     {
        //         var filePath = GlobalStatic.NOTE_FILES[alias];
        //         // does it exist? 
        //         if (!File.Exists(filePath))
        //         {
        //             DBg.d(LogLevel.Warning, $"File {filePath} does not exist.");
        //             return false;
        //         }
        //         else
        //         {
        //             // get the containing directory of the file
        //             var path = Path.GetDirectoryName(filePath);
        //             if (path == null)
        //             {
        //                 DBg.d(LogLevel.Error, $"Error: Unable to get directory name for {filePath}");
        //                 return false;
        //             }
        //             else
        //             {
        //                 // set the current directory to the file's directory
        //                 Directory.SetCurrentDirectory(path);
        //                 return true;
        //             }
        //         }
        //     }
        //     else
        //         {
        //             DBg.d(LogLevel.Warning, $"Alias {alias} not found.");
        //             return false;

        //         }

        //     }
        public void Search(string[] tokens)
        {
            DBg.d(LogLevel.Trace, $"Search: [{string.Join(" ", tokens)}]");
            // the first token of the inline isn't one of our key words
            // does it match one of the notefile aliases? 
            string inline = string.Empty;
            if (GlobalStatic.NOTE_FILES.ContainsKey(tokens[0]))
            {
                DBg.d(LogLevel.Trace, $"First token IS file alias: {tokens[0]}");
                var noteFile = new KeyValuePair<string, string>(tokens[0], GlobalStatic.NOTE_FILES[tokens[0]]);
                // remove the command from the inline
                // print the command tokens debug
                for (int i = 1; i < tokens.Count(); i++)
                {
                    DBg.d(LogLevel.Trace, $"token {i}: {tokens.ElementAt(i)}");
                }

                DBg.d(LogLevel.Trace, $"tokens.count: {tokens.Count()}");
                if ((tokens != null) && tokens.Count() > 0)
                {
                    inline = string.Join(" ", tokens.Skip(1));
                }
                else
                {
                    inline = string.Empty;
                }
                DBg.d(LogLevel.Trace, $"inline: {inline}");
                // if the remainder of the inline is empty, then just copy the contents of that file
                // to the console
                if (string.IsNullOrEmpty(inline))
                {
                    // read the file and print it to the console
                    //if(GlobalStatic.interactiveMode) {
                    Console.WriteLine($"======= {noteFile.Key} ({noteFile.Value}) ".PadRight(Console.WindowWidth - 1, '='));
                    //}
                    var fileLines = File.ReadAllLines(noteFile.Value);
                    foreach (var line in fileLines)
                    {
                        Console.WriteLine($"| {line}");
                    }
                    return;
                }



                if (!File.Exists(noteFile.Value))
                {
                    // make sure all directories in the path exist and if not, create them,
                    // then create the file
                    var directoryPath = Path.GetDirectoryName(noteFile.Value);
                    if (directoryPath != null && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        DBg.d(LogLevel.Information, $"Directory created: {directoryPath}");
                    }


                    // create the file
                    File.Create(noteFile.Value).Close();
                    DBg.d(LogLevel.Information, $"File created: {noteFile.Value}");
                }
                // Read all contents of the file
                string fileContents = File.ReadAllText(noteFile.Value);

                // Check if the file contents contain the string inline
                StringComparison comparisonType = GlobalStatic.PARAMETERS["CASE_SENSITIVE"] == "true"
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                // Determine the match mode (default to Contains if not set)
                string matchMode = GlobalStatic.PARAMETERS.ContainsKey("MATCH_MODE")
                    ? GlobalStatic.PARAMETERS["MATCH_MODE"]
                    : "Contains";

                var matchingLines = fileContents.Split(Environment.NewLine)
                    .Where(line =>
                    {
                        switch (matchMode.ToLower())
                        {
                            case "exact":
                                return string.Equals(line, inline, comparisonType);

                            case "any":
                                var searchTokens = tokens.Skip(1).ToArray(); // Skip file alias
                                return searchTokens.Any(token =>
                                    line.IndexOf(token, comparisonType) >= 0);

                            case "contains":
                            default:
                                return line.IndexOf(inline, comparisonType) >= 0;
                        }
                    })
                    .ToList();

                if (matchingLines.Any())
                {

                    foreach (var match in matchingLines)
                    {
                        Console.WriteLine($"{noteFile.Key} ({noteFile.Value}) == {match}");
                    }
                }

                else
                {
                    Console.WriteLine($"Adding '{inline}' to {noteFile.Key} ({noteFile.Value})");
                    // check the file exists and if not, create it

                    // Append the string to the file
                    using (StreamWriter sw = File.AppendText(noteFile.Value))
                    {
                        sw.WriteLine(inline);
                    }
                }


            }
            else
            {
                DBg.d(LogLevel.Trace, $"tokens.count: {tokens.Count()}");
                if ((tokens != null) && tokens.Count() > 0)
                {
                    inline = string.Join(" ", tokens);
                }
                else
                {
                    inline = string.Empty;
                }

                DBg.d(LogLevel.Trace, $"First token NOT file alias - searching for all [ {inline} ]");
                // greap ALL of the note files for this string
                foreach (var noteFile in GlobalStatic.NOTE_FILES)
                {
                    var filePath = noteFile.Value;
                    // Check if the file exists
                    if (!File.Exists(filePath))
                    {
                        //DBg.d(LogLevel.Warning, $"File {filePath} does not exist.");
                        break;
                    }

                    // Read all contents of the file
                    string fileContents = File.ReadAllText(filePath);

                    // Check if the file contents contain the string inline
                    StringComparison comparisonType = GlobalStatic.PARAMETERS["CASE_SENSITIVE"] == "true"
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;

                    // Determine the match mode (default to Contains if not set)
                    string matchMode = GlobalStatic.PARAMETERS.ContainsKey("MATCH_MODE")
                        ? GlobalStatic.PARAMETERS["MATCH_MODE"]
                        : "Contains";

                    var matchingLines = fileContents.Split(Environment.NewLine)
                        .Where(line =>
                        {
                            switch (matchMode.ToLower())
                            {
                                case "exact":
                                    return string.Equals(line, inline, comparisonType);

                                case "any":
                                    var searchTokens = tokens; // Use all tokens for global search
                                    return searchTokens.Any(token =>
                                        line.IndexOf(token, comparisonType) >= 0);

                                case "contains":
                                default:
                                    return line.IndexOf(inline, comparisonType) >= 0;
                            }
                        })
                        .ToList();

                    if (matchingLines.Any())
                    {

                        foreach (var match in matchingLines)
                        {
                            Console.WriteLine($"{noteFile.Key} ({filePath}) == {match}");
                        }
                    }
                    else
                    {
                        // do nothing. add has to be to an explicit file
                    }
                }

            }

        }
    }
}