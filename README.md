# hamnt
Hyper aggressively minimal note taking CLI / Interactive shell app.

Usage: hamnt [options]
Options:
  -a, --add <alias> <filepath>   Add a note file with the given alias and filepath
  -r, --remove <alias>           Remove a note file with the given alias (does NOT delete the file
  -l, --list                     List all note files
  -s, --set                      Set a (configuration) parameter
  -v, --version                  Show version information
  -q, --quit                     Quit the program
  -h, --help                     Show this help message

Launching hamnt with no arguments will enter interactive mode.

In interactive mode, you can use the single letter commands without the leading - or --
and uer input continues until `q`uit. 

If the first argument doesn't match any of the commands, but IS a Note File alias,
the rest of the commandline is added to that note file (which is created if applicable)
.. unless it already exists in which case it just tells you that. 

If the first argument doesn't match any of the command OR a Note File alias, 
all note files are grepped for the remainder of the command line.

