<div align="center">
  <img src="https://github.com/user-attachments/assets/0854dc58-c548-491b-9053-b070dfa5c19c" alt="Segugio" width="300" />
</div>

## Introduction
<b>Segugio</b> allows the execution and tracking of critical steps in the malware detonation process, from clicking on the first stage to extracting the malware's final stage configuration.<br>
<b>Segugio</b> was created to address the need for speeding up the extraction of IoCs from malicious artifacts within the analysis environment.<br>
Malware analysis often involves time-consuming activities like static and dynamic analysis, which require extensive knowledge in reverse engineering and code analysis.<br><br>
It is fully automated and designed to simplify the life of security analysts and specialists working in cyber incident response (DFIR), enabling them to quickly identify malicious artifacts without needing to perform complex static and dynamic analyses, and focus instead on behavioral analysis.

## Credits

First of all a special thank you to my dear friend [Antelox](https://github.com/Antelox) for his invaluable support during the development of Segugio. 
His insights, suggestions, and constant encouragement played a crucial role in the completion of this project. 
Thank you for sharing brilliant ideas and helping me improve every aspect of <b>Segugio</b>! 
<br><br>
I would like to express my deepest gratitude to [Airbus CERT](https://github.com/airbus-cert), the developer of [dnYara](https://github.com/airbus-cert/dnYara), which serves as part of the foundation of <b>Segugio</b>. 
Your library helped me saving so much time and its versatility made my work much easier. <br>
Thank you for your dedication and for sharing your work with the community!
<br><br>

> [!WARNING]
> Using <b>Segugio</b> for the analysis of malicious artifacts should only be done in dedicated malware analysis environments, such as sandboxes. Do not use Segugio in production environments, especially those without proper segregation, as they are not intended for analyzing malicious artifacts.
> - Segugio does not provide any protection from executing malicious software.
> - The analysis environment must always be reset to its initial state before conducting a new analysis.
> - The ability to correctly identify the malware family of an artifact depends on the performance of the analysis environment (CPU and RAM available) and the effectiveness of the YARA rules used.

## How it works
![segugio_starting-screen](https://github.com/user-attachments/assets/d26a18ca-ae41-42f8-bde7-e5025cc179a0)

Integration with the .NET wrapper for the yara.dll library by [airbus-cert](https://github.com/airbus-cert) (<b>a big thanks to them for their excellent work :clap:</b>) allows YARA rules to be used to search for indicators related to known malware families within the private memory of processes.
The functionality can be summarized in three key steps:
1.	The user selects a file to execute, possibly defining options such as the command line or the parent process of the one to be created.
2.	After clicking the Segugio button, Segugio starts scanning the process related to the selected file's execution, as well as its parent process (in the example, explorer.exe). Be aware that some systems might have multiple explorer.exe instances.
3.	Once Segugio identifies a process that matches a YARA rule, if a dedicated Python script for the identified YARA rule exists (e.g., AgentTesla), it begins dumping the process that matched the rule to automatically extract the malware's configuration from the private memory (for example, AgentTesla).

## Features

<b>Segugio</b> uses Parent Process ID (PPID) Spoofing to launch the artifact to be analyzed from a set of arbitrary parent processes. This allows the artifact to appear as if it is being executed by a user, while Segugio tracks the kill chain and identifies various stages.<br>
<b>Segugio</b> allows the execution of files of any type through a customized command line. This feature is useful if you want to execute a DLL using the rundll32 utility and possibly invoke a specific export.<br>
<b>Segugio</b> provides a tree view of processes involved after a file execution. <br><br>
The graphical interface provides real-time summaries of the following information for each process involved in the file’s execution:
- Process name and associated PID
- Process status (running / terminated)
- Process Command Line
- Malware family (if matched by yara rule)
- Any configurations extracted from memory (if config extractor is present)
With these features, it becomes easier to trace malicious behavior across processes (e.g., if the malware performs injection into other processes or reveals malicious stages).

## Segugio at work

Below you'll find some screenshot while <b>Segugio</b> was inspecting malwares like Adwind, Remcos and Formbook (XLoader).

<table>
  <tr>
    <td align="center">
    <a href="https://github.com/reecdeep/segugio/blob/master/segugio_adwind_sample.png?raw=true">
      <img src="https://github.com/reecdeep/segugio/blob/master/segugio_adwind_sample.png" alt="Adwind RAT config extraction" style="width: 100px; height: auto;" />
    </a>
    <br>
    <em>Adwind RAT config extraction</em>
 </td>
    <td align="center">
    <a href="https://github.com/reecdeep/segugio/blob/master/segugio_powershell_staging.png?raw=true">
      <img src="https://github.com/reecdeep/segugio/blob/master/segugio_powershell_staging.png" alt="Powershell staging commandline" style="width: 100px; height: auto;" />
    </a>
    <br>
    <em>Powershell staging commandline</em>
 </td>
    <td align="center">
    <a href="https://github.com/reecdeep/segugio/blob/master/segugio_remcos_sample.png?raw=true">
      <img src="https://github.com/reecdeep/segugio/blob/master/segugio_remcos_sample.png" alt="RemCos RAT config extraction" style="width: 100px; height: auto;" />
    </a>
    <br>
    <em>RemCos RAT config extraction</em>
  </td>
    <td align="center">
    <a href="https://github.com/reecdeep/segugio/blob/master/segugio_xloader_sample.png?raw=true">
      <img src="https://github.com/reecdeep/segugio/blob/master/segugio_xloader_sample.png" alt="XLoader stealer C&C extraction" style="width: 100px; height: auto;" />
    </a>
    <br>
    <em>XLoader stealer C&C extraction</em>
    </td>
  </tr>
</table>

## First Run - read carefully!

Before running <b>Segugio</b> for the first time, it is necessary to complete the configuration in the settings.ini file. The configuration requires the following parameters, which are mandatory for the program to function properly:
- `YaraRulesDirectory` The absolute path to the folder containing the YARA rules
- `PythonExecutablePath` The absolute path to the folder where Python (python.exe) is located
- `ConfigExtractorsDirectory` The absolute path to the folder containing Python scripts for configuration extraction
- `DumpFolder` The absolute path to the folder where memory dumps of processes that match YARA rules will be saved

Additionally, the following parameters can be modified:
- `DefaultCommandlines` A list of command lines frequently used with specific file types. Multiple default command lines can be defined, separated by a pipe (|) character.
- `PreferredParentProcess` The preferred parent process for the created process when executing the file. Some malware checks the parent process. In MS Windows, if a user executes a file, the related process will be a child of Explorer.exe.
- `MonitorInterval` The interval in milliseconds for background process monitoring updates (recommended value: 100).
- `ScanInterval` The interval in milliseconds for scanning process memory (recommended value: 1000).
- `isEnabledParentScan` Allows you to choose whether to scan the parent process of the created child process.

At its core, Segugio relies on YARA and automatic configuration extractors, which are located in the program's config folder.

## YARA and Configuration Extractors
<b>Segugio</b> ability to identify and associate processes with malware families depends on the quality of the YARA rules present in the folder specified by the YaraRulesDirectory parameter. Therefore, it’s highly recommended to learn how to write good YARA rules!
<b>Segugio</b> also makes use of configuration extractors (optional) to automatically extract critical configuration data for malware operation from memory.
However, in order to associate an extractor with a YARA rule, Segugio needs the YARA rule to contain well-defined meta fields, structured like this:


```console
meta:
        name = "YARA_Rule_Name"
        description = "YARA_Rule_Description"
```

Similarly, Python scripts must have the following header at the beginning of the code:

```console
# -*- coding: utf-8 -*-
__author__ = "author"
__version__ = "1.0"
__script_name__ = "YARA_Rule_Name"
```

As you can see, to associate a python script with a YARA rule, the script_name parameter in the python script header must match the YARA rule name.
The respective file names of the YARA rule and the configuration extractor can, of course, be different!
When <b>Segugio</b> starts, it checks the formal correctness of YARA rules and configuration extractors (.py), and attempts to associate them. If a YARA rule and a configuration extractor have matching names, Segugio will use the configuration extractor when a match between a process and the YARA rule is found.
After starting the program, YARA rules are compiled and ready to be used within Segugio.

As a demonstration of Segugio’s functionality, I have released a couple of configuration extractors along with a few YARA rules.
Please note that the YARA rules and configuration extractors included in this repository may not be up-to-date or fully functional. They are intended solely as a proof of concept for how Segugio operates.

## Future Improvements:
    - Network fingerprinting with JA3, JA4, JARM
    - Use of YARAX
    - Exporting the identified cyber kill chain
    - Collecting all IoCs identified during the execution stages
    - Identifying the type of process injection (using YARA)
    - Kernel-level development


## External Dependences
Segugio relies upon the folliwing libraries:
-   dnYara by airbus-cert [dnYara by airbus-cert](https://github.com/airbus-cert/dnYara)
-   Python 3 [Python 3](https://www.python.org/downloads/)
-   .NET Framework 4.72 [.NET Framework 4.72](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)

Made with :heartpulse: in Italy :it:
