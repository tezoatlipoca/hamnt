using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.IO;
using System.Collections.Generic; // Ensure this is included for file operations


class Program
{
    static void Main(string[] args)
    {
        // Set up cleanup handlers
        Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleCleanup);
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(HandleProcessExit);



        // Application logic and command-line argument processing goes here
        Console.WriteLine($"HAMNT - Hyper Aggressively Minimal Note Taking app // VERSION: {GlobalStatic.bldVersion}");

        if (args.Length > 0)
        {
            DBg.d(LogLevel.Trace, "Command-line arguments:");
            foreach (var arg in args)
            {
                DBg.d(LogLevel.Trace, "Command-line argument: " + arg);
            }
        }

        var command = string.Empty;
        // default mode is continuous - loop asking for user input, quit on q
        // a - adds a note file with provided (and quoted) "alias" and "filepath"
        // r - removes a note file with provided (and quoted) "alias"
        // l - lists all note files
        // q - quits the program
        //      
        while (command != "q")
        {
            Console.WriteLine("(a/r/l/q) >>");
            var inline = Console.ReadLine();
            command = inline?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(command))
            {
                continue;
            }
            else if (command == "a")
            {
                Console.WriteLine("Enter alias: ");
                var alias = Console.ReadLine();
                if (!string.IsNullOrEmpty(GlobalStatic.NOTES_LOCATION))
                {
                    Console.WriteLine($"Enter filename (in {GlobalStatic.NOTES_LOCATION}) or absolute: ");
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
                    if (GlobalStatic.NOTES_LOCATION != null)
                    {
                        filePath = Path.Combine(GlobalStatic.NOTES_LOCATION, filePath);
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
            else if (command == "r")
            {
                Console.WriteLine("Enter alias to remove: ");
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
            else if (command == "l")
            {
                foreach (var noteFile in GlobalStatic.NOTE_FILES)
                {
                    Console.WriteLine($"{noteFile.Key} -> {noteFile.Value}");
                }
            }
            else if (command == "s")
            {
                // set the notes location
                Console.WriteLine("Enter notes location: ");
                var notesLocation = Console.ReadLine();
                if (notesLocation != null)
                {
                    GlobalStatic.NOTES_LOCATION = notesLocation;
                    DBg.d(LogLevel.Trace, $"Set notes location: {notesLocation}");
                }
                GlobalStatic.WriteConfigFile();
            }
            else if (command == "q")
            {
                break;
            }
            else if (command == "v")
            {
                Console.WriteLine($"Version: {GlobalStatic.bldVersion}");
            }
            else
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
        // we're wrapping up 
        // write the note files back to the note files config file
        GlobalStatic.WriteNoteFiles();
        cleanupPerformed = true;
        Console.WriteLine("Exiting program.");



    }

    private static bool cleanupPerformed = false;
    private static void HandleCleanup(object? sender, ConsoleCancelEventArgs e)
    {
        if (cleanupPerformed) return;
        // Prevent the process from terminating immediately
        e.Cancel = true;

        Console.WriteLine("Received termination signal, cleaning up...");

        // Save note files
        GlobalStatic.WriteNoteFiles();
        cleanupPerformed = true;
        //DBg.d(LogLevel.Trace, "Cleanup complete, exiting.");

        // Now we can exit
        Environment.Exit(0);
    }

    private static void HandleProcessExit(object? sender, EventArgs e)
    {
        if (cleanupPerformed) return;
        // This handler catches other termination scenarios
        Console.WriteLine("Process exiting, performing final cleanup...");
        GlobalStatic.WriteNoteFiles();
    }
}
