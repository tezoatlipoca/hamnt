using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ComponentModel.Design;


namespace hamnt
{
    public class hamntEngine
    {

        public void AddNoteFile(string inline)
        {
            DBg.d(LogLevel.Trace, "Adding note file");
            Console.WriteLine("Enter alias: ");
            var alias = Console.ReadLine();
            if (!string.IsNullOrEmpty(GlobalStatic.PARAMETERS["NOTES_LOCATION"]))
            {
                Console.WriteLine($"Enter filename (in {GlobalStatic.PARAMETERS["NOTES_LOCATION"]}) or absolute: ");
            }
            else
            {
                Console.WriteLine("Enter absolute filename or file in current directory.");
            }
            var filePath = Console.ReadLine();
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
            DBg.d(LogLevel.Trace, $"Added note file: {alias} -> {filePath}");



            if (!File.Exists(filePath))
            {
                // create the file
                //File.Create(filePath).Close();
                DBg.d(LogLevel.Warning, $"File does not exist but will if something is added to it: {filePath}");
            }
            else
            {
                //DBg.d(LogLevel.Trace, $"Note file already exists: {filePath}");
            }
        }
        public void RemoveNoteFile(string inline)
        {
            DBg.d(LogLevel.Trace, "Removing note file");
            Console.WriteLine("Enter alias: ");
            var alias = Console.ReadLine();
            if (GlobalStatic.NOTE_FILES.ContainsKey(alias))
            {
                GlobalStatic.NOTE_FILES.Remove(alias);
                DBg.d(LogLevel.Trace, $"Removed note file: {alias}");
            }
            else
            {
                DBg.d(LogLevel.Warning, $"Alias {alias} not found.");
            }


        }

        public void ListNoteFiles(string inline)
        {
            DBg.d(LogLevel.Trace, "Listing note files");
            foreach (var noteFile in GlobalStatic.NOTE_FILES)
            {
                Console.WriteLine($"{noteFile.Key} -> {noteFile.Value}");
            }
        }
        public void SetParameter(string inline)
        {
            bool didAnytingChange = false;
            var paramName = string.Empty;
            var paramValue = string.Empty;
            // if we get here via cli, the parameter name will be 2nd param, value 3rd
            // if we get here via interactive mode we can prompt
            if (GlobalStatic.interactiveMode)
            {
                Console.WriteLine("Enter parameter name: ");
                paramName = Console.ReadLine();
                Console.WriteLine("Enter parameter value: ");
                paramValue = Console.ReadLine();
            }
            else
            {
                var tokens = inline.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2)
                {
                    Console.WriteLine("Error: Not enough parameters.");
                    return;
                }
                paramName = tokens[1];
                paramValue = tokens[2];
            }
            // the above can't handle cases where parameter names NOR parameter values are quoted/include spaces.

            // check to see if the parameter name is not null/empty
            if (string.IsNullOrEmpty(paramName))
            {
                Console.WriteLine("Error: Parameter name is null or empty.");
                return;
            }

            switch (paramName.ToUpper())
            {
                case "NOTES_LOCATION":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        Console.WriteLine("Error: No NOTES_LOCATION provided.");
                        return;
                    }
                    else
                    {
                        // check to see if the parameter value is a valid path
                        if (!Path.IsPathRooted(paramValue))
                        {
                            Console.WriteLine($"Warning: NOTES_LOCATION '{paramValue}' is not a valid path; will be created if needed");

                        }

                        // check to see if the parameter value is a directory
                        if (!Directory.Exists(paramValue))
                        {
                            Console.WriteLine($"Warning: NOTES_LOCATION '{paramValue}' is not a directory.");

                        }
                        GlobalStatic.PARAMETERS["NOTES_LOCATION"] = paramValue;
                        didAnytingChange = true;
                        DBg.d(LogLevel.Trace, $"Set NOTES_LOCATION: {paramValue}");
                    }
                    break;
                case "LOG_LEVEL":
                    // check to see if the parameter value is not null/empty
                    if (string.IsNullOrEmpty(paramValue))
                    {
                        Console.WriteLine("Error: LOG_LEVEL is null or empty.");
                        return;
                    }
                    // check to see if the parameter value is a valid LogLevel
                    if (!Enum.TryParse(typeof(LogLevel), paramValue, true, out _))
                    {
                        Console.WriteLine($"Error: LOG_LEVEL '{paramValue}' is not valid. Try one of: {string.Join(", ", Enum.GetNames(typeof(LogLevel)))}.");
                        return;
                    }
                    else
                    {
                        // set the parameter value
                        GlobalStatic.SetLogLevel((LogLevel)Enum.Parse(typeof(LogLevel), paramValue));
                        didAnytingChange = true;
                    }
                    break;
                default:
                    // do nothing
                    break;
            }


            if (didAnytingChange)
            {
                GlobalStatic.WriteConfigFile();
            }
        }

        public void Search(string inline, string command)
        {
            // the first token of the inline isn't one of our key words
            // does it match one of the notefile aliases? 
            if (GlobalStatic.NOTE_FILES.ContainsKey(command))
            {
                var noteFile = new KeyValuePair<string, string>(command, GlobalStatic.NOTE_FILES[command]);
                // remove the command from the inline
                var commandtokens = inline?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1);
                inline = string.Join(" ", commandtokens);

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

                // Check if the file contents contain the string inline (case-insensitive)
                if (fileContents.Split(Environment.NewLine)
                                .Any(line => line.Equals(inline, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"{noteFile.Key} ({noteFile.Value}) == {inline}");
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

                    // Check if the file contents contain the string inline (case-insensitive)
                    var matchingLines = fileContents.Split(Environment.NewLine)
                                                    .Where(line => line.IndexOf(inline, StringComparison.OrdinalIgnoreCase) >= 0)
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