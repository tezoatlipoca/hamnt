using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.IO;
using hamnt; // Ensure this is included for file operations


class Program
{
    static void Main(string[] args)
    {
        // handle our self extraction if we're running as a snap
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            Environment.SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", 
                Path.Combine(Environment.GetEnvironmentVariable("SNAP_USER_COMMON")!, "dotnet_extract"));
                
            // Ensure extraction directory exists
            var extractDir = Path.Combine(Environment.GetEnvironmentVariable("SNAP_USER_COMMON")!, "dotnet_extract");
            if (!Directory.Exists(extractDir))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(extractDir);
            }
        }
        else if (OperatingSystem.IsWindows())
        {
            // On Windows, use a directory in the user's AppData folder
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string extractDir = Path.Combine(appDataPath, "hamnt", "dotnet_extract");
            
            Environment.SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", extractDir);
            
            if (!Directory.Exists(extractDir))
            {
                Directory.CreateDirectory(extractDir);
            }
        }
        // Set up cleanup handlers
        Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleCleanup);
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(HandleProcessExit);

        // Application logic and command-line argument processing goes here

        if (args.Length > 0)
        {
            DBg.d(LogLevel.Trace, "Command-line arguments:");
            foreach (var arg in args)
            {
                DBg.d(LogLevel.Trace, "Command-line argument: " + arg);
            }
        }
        hamntEngine engine = new hamntEngine();

        // if there are command line arguments execute in CLI mode
        // if not, slip into interactive mode

        if (args.Length > 0)
        {
            // execute in CLI mode
            GlobalStatic.interactiveMode = false;
        }
        else
        {
            Console.WriteLine($"HAMNT - Hyper Aggressively Minimal Note Taking app // VERSION: {GlobalStatic.bldVersion}");

        }
        // we can re-use the while { switch } loop for CLI mode by  
        // setting command <-- q if we're not in interactive mode
        var command = string.Empty;
        var reason = string.Empty;
        // default mode is continuous - loop asking for user input, quit on q
        // a - adds a note file with provided (and quoted) "alias" and "filepath"
        // r - removes a note file with provided (and quoted) "alias"
        // l - lists all note files
        // q - quits the program
        //      
        while (command != "q")
        {
            if (GlobalStatic.interactiveMode)
            {
                Console.Write("(a/r/l/s/e/v/h/q) > ");
            }
            var inline = string.Empty;
            if (GlobalStatic.interactiveMode)
            {
                // we're in interactive mode
                inline = Console.ReadLine();
            }
            else
            {
                // we're in CLI mode
                inline = string.Join(" ", args); // this trashes any previous spaces. 
            }

            var tokens = inline?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(token => token.StartsWith("\"") && token.EndsWith("\"") ? token.Trim('"') : token)
                                .ToArray() ?? Array.Empty<string>();
            command = tokens.FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(command))
            {
                reason = "Nothing to do/no args.";
                continue;
            }
            switch (command)
            {
                case "--add":
                case "a":
                case "-a":
                    {
                        engine.AddNoteFile(tokens);
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }

                case "--remove":
                case "-r":
                case "r":
                    {
                        engine.RemoveNoteFile(tokens);
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }

                case "--list":
                case "-l":
                case "l":
                    {
                        engine.ListNoteFiles();
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }
                case "--edit":
                case "-e":
                case "e":
                    {
                        engine.EditNoteFile(tokens);
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }
                // case "--cd":
                // case "-c":
                // case "c":
                //     {
                //         if(engine.ChangeDirectory(tokens)) {
                //             command = "q";
                //             break;
                //         } else {
                            
                //         };
                //         if (!GlobalStatic.interactiveMode) { command = "q"; }
                //         ;
                //         break;
                //     }
                case "--set":
                case "-s":
                case "s":
                    {
                        engine.SetParameter(tokens);
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }
                case "--version":
                case "-v":
                case "v":
                    {
                        Console.WriteLine($"Version: {GlobalStatic.bldVersion}");
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }
                case "--quit":
                case "-q":
                case "q":
                    {
                        reason = "Quit command given.";
                        break;
                    }

                case "--help":
                case "-h":
                case "h":
                    {
                        Console.WriteLine("Usage: hamnt [options]");
                        Console.WriteLine("Options:");
                        Console.WriteLine("  -a, --add <alias> <filepath>   Add a note file with the given alias and filepath");
                        Console.WriteLine("  -r, --remove <alias>           Remove a note file with the given alias (does NOT delete the file");
                        Console.WriteLine("  -l, --list                     List all note files");
                        Console.WriteLine("  -s, --set                      Set a (configuration) parameter");
                        Console.WriteLine("  -v, --version                  Show version information");
                        Console.WriteLine("  -e, --edit <alias>                    Edit a note file (alias) in the default editor");
                        Console.WriteLine("  -q, --quit                     Quit the program");
                        Console.WriteLine("  -h, --help                     Show this help message\n");
                        Console.WriteLine("Launching hamnt with no arguments will enter interactive mode.");
                        Console.WriteLine("In interactive mode, you can use the single letter commands without the leading - or --\n");
                        Console.WriteLine("If the first argument doesn't match any of the commands, but IS a Note File alias,\n the rest of the commandline is added to that note file (which is created if applicable).\n");
                        Console.WriteLine("If the first argument doesn't match any of the command OR a Note File alias, \nall note files are grepped for the remainder of the command line.\n");
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        ;
                        break;
                    }

                default:
                    {
                        DBg.d(LogLevel.Trace, "--> search [" + string.Join(" ", tokens) + "]");
                        engine.Search(tokens);
                        if (!GlobalStatic.interactiveMode) { command = "q"; }
                        break;
                    }
            } // end switch




        } // end while
        // we're wrapping up 
        // write the note files back to the note files config file
        if (GlobalStatic.noteFilesChanged)
        {
            GlobalStatic.WriteNoteFiles();
        }
        cleanupPerformed = true;
        if (string.IsNullOrEmpty(reason))
        {
            //Console.WriteLine("Exiting program.");
        }
        else
        {
            Console.WriteLine($"Exiting program: {reason}");
        }


    }

    private static bool cleanupPerformed = false;
    private static void HandleCleanup(object? sender, ConsoleCancelEventArgs e)
    {
        if (cleanupPerformed) return;
        // Prevent the process from terminating immediately
        e.Cancel = true;

        Console.WriteLine("Received termination signal, cleaning up...");

        // Save note files
        if (GlobalStatic.noteFilesChanged)
        {
            GlobalStatic.WriteNoteFiles();
        }
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
        if (GlobalStatic.noteFilesChanged)
        {
            // Save note files
            GlobalStatic.WriteNoteFiles();
        }

    }
}
