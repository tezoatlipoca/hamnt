# hamnt
**H**yper **A**ggressively **M**inimal **N**ote **T**aking CLI / Interactive shell app.

There are lots of good note apps (Silver Bullet etc.), but I wanted the 
ability to rapidly jot notes to completely disparate files without 
switching tabs, opening/closing files; or rapid grep through a bunch
of files which may be in completely different locations. 

Note files are _aliased_ so I don't have to care where they are,
vs where I am in order to view, search or modify them. 
e.g.: add `tomatoes` to my grocery list:
```bash
tezoatlipoca@pickles17:~$ hamnt grocery tomatoes
Adding 'tomatoes' to grocery (/home/tezoatlipoca/Dropbox/grocery_list.txt)
```
The same actions+parameters work via command line or 
in a really basic interactive shell (good for rapid note taking
w/o having to type `hamnt` over and over again).

![Version](https://img.shields.io/github/v/tag/tezoatlipoca/hamnt?label=version)

## Table of Contents
- [Usage](#usage)
- [Configuration Parameters](#configuration-parameters)
- [Data Store](#data-store)
- [Installation](#installation)
- [Usage Walkthrough](#walkthrough)

## Usage

```bash
Usage: hamnt [options]
Options:
  -a, --add <alias> <filepath>   Add a note file with the given alias and filepath
  -r, --remove <alias>           Remove a note file with the given alias (does NOT delete the file
  -l, --list                     List all note files
  -s, --set                      Set a (configuration) parameter
  -v, --version                  Show version information
  -q, --quit                     Quit the program
  -h, --help                     Show this help message
```
Launching hamnt with no arguments will enter interactive mode.

In interactive mode, you can use the single letter commands without the leading - or --
and uer input continues until `q`uit; in interactive mode, you don't have to specify
any parameters for commands, you will be prompted for them.

If the first argument doesn't match any of the commands, but IS a Note File alias,
the rest of the commandline is added to that note file (which is created if applicable)
.. unless it already exists in which case it just tells you that. 

If the first argument doesn't match any of the commands OR a Note File alias, 
all note files are grepped for the remainder of the command line.

## Configuration Parameters
* LOG_LEVEL      - change to Trace, Debug, Information, Warning, Error, Fatal, None
* NOTES_LOCATION - the default place where any new aliased notes files will be created; Has no impact on any file aliases already defined.

## Data Store
In `<user home>/.config/hamnt`, `hamnt` stores 
* `hamnt.config.json` - configuration parameters above
* `hamnt.notefiles.json` - all of your note file aliases
These are read on launch and updated when `hamnt` quits. 

## Installation
### From Release
Download the latest release for your platform from the [releases page](https://github.com/tezoatlipoca/hamnt/releases).

### From Source
```bash
git clone https://github.com/tezoatlipoca/hamnt.git
cd hamnt
dotnet publish -c Release
```

## Walkthrough
Launch hamnt:
```bash
tezoatlipoca@pickles17:~$ hamnt
2025-04-11T23:31:02 WARN  | Creating config file directory: /home/tezoatlipoca/.config/hamnt/hamnt.config.json
2025-04-11T23:31:02 WARN  | Creating config file: /home/tezoatlipoca/.config/hamnt/hamnt.config.json
HAMNT - Hyper Aggressively Minimal Note Taking app // VERSION: 0.0.1+543cc8ff4a509d1b2956a501b9ffc3176538ad9e
(a/r/l/q) >>
```
The default location for note files is `<user home>/.config/hamnt/notes`. Create a new notes file alias:
```bash
(a/r/l/q) >>
a
Enter alias: 
foo
Enter filename (in /home/tezoatlipoca/.config/hamnt/notes) or absolute: 
foo_notes.txt
2025-04-11T23:32:25 INFO  | Added note file: foo -> /home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt
2025-04-11T23:32:25 WARN  | File does not exist but will if something is added to it: /home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt
```
Now lets add some content to that file
```bash
(a/r/l/q) >>
foo This text is added to foo_notes.txt
2025-04-11T23:34:55 INFO  | Directory created: /home/tezoatlipoca/.config/hamnt/notes
2025-04-11T23:34:55 INFO  | File created: /home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt
Adding 'This text is added to foo_notes.txt' to foo (/home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt)
```
Now, if we just give the name of the file alias `foo`, it dumps the contents of that file:
```bash
(a/r/l/q) >>
foo
======= foo (/home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt) ===========================================================
| This text is added to foo_notes.txt
```
Lets change the (default) location where notes files are kept; don't worry, the new folder will be created later when it is needed
```bash
a/r/l/q) >>
s
Enter parameter name: 
NOTES_LOCATION
Enter parameter value: 
/home/tezoatlipoca/note_vault
2025-04-11T23:37:35 WARN  | NOTES_LOCATION '/home/tezoatlipoca/note_vault' is not a directory.
```
Now when we add a new file alias, its defined in the new location. 
```bash
(a/r/l/q) >>
a      
Enter alias: 
todo
Enter filename (in /home/tezoatlipoca/note_vault) or absolute: 
todos.txt
2025-04-11T23:38:36 INFO  | Added note file: todo -> /home/tezoatlipoca/note_vault/todos.txt
2025-04-11T23:38:36 WARN  | File does not exist but will if something is added to it: /home/tezoatlipoca/note_vault/todos.txt
```
The actual note file isn't created until content is first added though:
```bash
(a/r/l/q) >>
todo This text should show up in /note_vault/todos.txt
2025-04-11T23:41:08 INFO  | Directory created: /home/tezoatlipoca/note_vault
2025-04-11T23:41:08 INFO  | File created: /home/tezoatlipoca/note_vault/todos.txt
Adding 'This text should show up in /note_vault/todos.txt' to todo (/home/tezoatlipoca/note_vault/todos.txt)
```
But what note file aliases are in use? 
```bash
(a/r/l/q) >>
l
foo -> /home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt
todo -> /home/tezoatlipoca/note_vault/todos.txt
```
Just entering some search text shows me which of my alias'd files contain it:
```bash
(a/r/l/q) >>
This text
foo (/home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt) == This text is added to foo_notes.txt
todo (/home/tezoatlipoca/note_vault/todos.txt) == This text should show up in /note_vault/todos.txt
```
When I quit, the list of all aliased note files and any configuration parameters are saved for next time.
I can also perform the same actions but as command line parameters:
```bash
ezoatlipoca@pickles17:~$ hamnt This text
foo (/home/tezoatlipoca/.config/hamnt/notes/foo_notes.txt) == This text is added to foo_notes.txt
todo (/home/tezoatlipoca/note_vault/todos.txt) == This text should show up in /note_vault/todos.txt
tezoatlipoca@pickles17:~$ hamnt a grocery /home/tezoatlipoca/Dropbox/grocery_list.txt
2025-04-11T23:46:36 INFO  | Added note file: grocery -> /home/tezoatlipoca/Dropbox/grocery_list.txt
2025-04-11T23:46:36 WARN  | File does not exist but will if something is added to it: /home/tezoatlipoca/Dropbox/grocery_list.txt
tezoatlipoca@pickles17:~$ hamnt grocery lettuce
2025-04-11T23:46:53 INFO  | File created: /home/tezoatlipoca/Dropbox/grocery_list.txt
Adding 'lettuce' to grocery (/home/tezoatlipoca/Dropbox/grocery_list.txt)
tezoatlipoca@pickles17:~$ hamnt grocery hamburgers
Adding 'hamburgers' to grocery (/home/tezoatlipoca/Dropbox/grocery_list.txt)
tezoatlipoca@pickles17:~$ hamnt grocery hamburgers
grocery (/home/tezoatlipoca/Dropbox/grocery_list.txt) == hamburgers
tezoatlipoca@pickles17:~$ hamnt grocery
======= grocery (/home/tezoatlipoca/Dropbox/grocery_list.txt) ================================================================
| lettuce
| hamburgers
```
````







