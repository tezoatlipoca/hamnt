(Just in case my release notes get gobbled if I retag on some workflow / build changes)


# v0.2.0

Fixed:
#9 - more "verbose" output in all log levels reduced, but configurable w/ VERBOSE_OUTPUT true/false
#8  - traps most file I/O errors and gives a meaningful msg instead of the exception when writing a notes file
#4 - new edit command: launches the specified alias using your OS's default file handler; or `export EDITOR=<editor of choice e.g. vi>`; if the editor is vi or emacs etc. `hamnt` blocks waiting for that to exit.. 
#7 - default NOTES_LOCATION wasn't in `%AppData%\hamnt` in Windows
#3  - file search sensitivity can be toggled with CASE_SENSITIVE true/false (default false)
#2 - in interactive mode, if you give a command with the requisite # of parameters it no longer prompts you for the parameters interactively

Search pattern matching can now be switched using MATCH_MODE:
- Contains  - results will be any note alias file lines that _contain_ the input you provide
- Exact - results will be any note alias file lines that _exactly match_ the input you provide
- Any - results will be any note alias file lines that contain _any of the words you provide in input_.

Also tightened up the command line a bit