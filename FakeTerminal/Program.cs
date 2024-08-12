using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net.Mime;

public class Program
{
    public static void Main(string[] args)
    {
        Clear();
        bool limitedMode = true;

        try
        {
            limitedMode = false;
        }
        catch (Exception)
        {
            Console.WriteLine("Could not get all custom libraries to run, please run the command 'install_requirements' to get the full thing with all custom commands");
            limitedMode = true;
        }

        CoolSystemCrap coolSystemCrap = new CoolSystemCrap();
        CommandSystem customCommand = new CommandSystem();
        ColorText coolColors = new ColorText();
        CommandFunctions commandFunctions = new CommandFunctions(coolSystemCrap, coolColors, customCommand, limitedMode);
        
        customCommand.AddCommand("help", "Get all of the commands built into the script", "important", commandFunctions.Help, false);
        customCommand.AddCommand("restart", "Restart the script", "system", Restart, false);
        customCommand.AddCommand("exit", "Exits the terminal", "system", Action=>Environment.Exit(0), false);
        customCommand.AddCommand("cd", "Changing directory with the C# script", "system", commandFunctions.ChangeDirectory, false);
        customCommand.AddCommand("getip", "Get your current public IP", "network", commandFunctions.GetCurrentIpAddress, false);
        customCommand.AddCommand("getdeviceip", "Get your current device IP", "network", commandFunctions.GetDeviceIpAddress, false);

        Console.WriteLine(coolColors.Colorize("Welcome to Dia Terminal, this project is meant to be used to get around terminal restrictions while having extra cool commands", "cyan"));

        while (true)
        {
            Console.Write($"{coolSystemCrap.GetUserName()}@{coolSystemCrap.GetHostname()} <({coolSystemCrap.GetPath()})> ");
            string command = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(command))
            {
                customCommand.ProcessCommand(command);
            }
        }
    }

    public static void Clear()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("cmd.exe", "/c cls").WaitForExit();
        }
        else
        {
            Console.Clear();
        }
    }

    public static void Restart(string[] args = null)
    {
        Console.WriteLine("Restarting script...");
        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
        Environment.Exit(0);
    }
}

public class CoolSystemCrap
{
    public string GetUserName()
    {
        return Environment.UserName;
    }

    public string GetHostname()
    {
        return Dns.GetHostName();
    }

    public void PathChanger(string path)
    {
        if (path == ".." || path == ",,")
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Directory.GetParent(currentDirectory).FullName);
        }
        else
        {
            if (Directory.Exists(path))
            {
                Directory.SetCurrentDirectory(path);
            }
            else
            {
                Console.WriteLine("Not a real path");
            }
        }
    }

    public string GetPath()
    {
        return Directory.GetCurrentDirectory();
    }
}

public class CommandSystem
{
    private Dictionary<string, List<CommandTemplate>> customCommandList = new Dictionary<string, List<CommandTemplate>>();
    public List<string> History { get; private set; } = new List<string>();

    public void AddCommand(string command, string description, string category, Action<string[]> method, bool customLibs)
    {
        if (!customCommandList.ContainsKey(category))
        {
            customCommandList[category] = new List<CommandTemplate>();
        }

        CommandTemplate commandTemplate = new CommandTemplate
        {
            Command = command,
            Description = description,
            CustomLibs = customLibs,
            Function = method
        };

        customCommandList[category].Add(commandTemplate);
    }

    public void RemoveCommand(string command)
    {
        foreach (var category in customCommandList.Values)
        {
            category.RemoveAll(c => c.Command == command);
        }
    }

    public bool CheckCommand(string command)
    {
        foreach (var category in customCommandList.Values)
        {
            if (category.Exists(c => c.Command == command))
            {
                return true;
            }
        }
        return false;
    }

    public Dictionary<string, List<CommandTemplate>> CommandList()
    {
        return customCommandList;
    }

    public bool RequiresCustomLibs(string command)
    {
        foreach (var category in customCommandList.Values)
        {
            CommandTemplate cmd = category.Find(c => c.Command == command);
            if (cmd != null)
            {
                return cmd.CustomLibs;
            }
        }
        return false;
    }

    public void ProcessCommand(string command)
    {
        string[] commandSplit = command.Split(" ");

        if (!History.Contains(command))
        {
            History.Add(command);
        }

        foreach (var category in customCommandList.Values)
        {
            CommandTemplate cmd = category.Find(c => c.Command == commandSplit[0]);
            if (cmd != null)
            {
                cmd.Function(commandSplit.Length > 1 ? commandSplit[1..] : Array.Empty<string>());
                return;
            }
        }

        Process.Start("/bin/bash", $"-c \"{command}\"")?.WaitForExit();
    }
}

public class CommandTemplate
{
    public string Command { get; set; }
    public string Description { get; set; }
    public bool CustomLibs { get; set; }
    public Action<string[]> Function { get; set; }
}

public class ColorText
{
    private readonly Dictionary<string, string> Colors = new Dictionary<string, string>
    {
        { "black", "\u001b[30m" },
        { "red", "\u001b[31m" },
        { "green", "\u001b[32m" },
        { "yellow", "\u001b[33m" },
        { "blue", "\u001b[34m" },
        { "magenta", "\u001b[35m" },
        { "cyan", "\u001b[36m" },
        { "white", "\u001b[37m" },
        { "reset", "\u001b[39m" }
    };

    public string Colorize(string text, string color = null)
    {
        if (color != null && Colors.ContainsKey(color))
        {
            text = Colors[color] + text + Colors["reset"];
        }
        return text;
    }
}

public class CommandFunctions
{
    private readonly CoolSystemCrap coolSystemCrap;
    private readonly ColorText coolColors;
    private readonly CommandSystem customCommand;
    private readonly bool limitedMode;

    public CommandFunctions(CoolSystemCrap coolSystemCrap, ColorText coolColors, CommandSystem customCommand, bool limitedMode)
    {
        this.coolSystemCrap = coolSystemCrap;
        this.coolColors = coolColors;
        this.customCommand = customCommand;
        this.limitedMode = limitedMode;
    }
    
    public void ChangeDirectory(string[] args)
    {
        coolSystemCrap.PathChanger(args[0]);
    }

    public void GetCurrentIpAddress(string[] args)
    {
        using (var client = new WebClient())
        {
            string ip = client.DownloadString("https://api.ipify.io");
            Console.WriteLine(coolColors.Colorize(ip, "cyan"));
        }
    }

    public void GetDeviceIpAddress(string[] args = null)
    {
        string hostName = Dns.GetHostName();
        
        var ipAddresses = Dns.GetHostAddresses(hostName);
        
        var ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        
        if (ipv4Address != null)
        {
            Console.WriteLine(ipv4Address.ToString());
        }
        else
        {
            Console.WriteLine("IPv4 address not found.");
        }
    }

    public void Help(string[] args)
    {
        Console.WriteLine(coolColors.Colorize("====== Help Command ======", "blue"));
        foreach (var category in customCommand.CommandList())
        {
            string categoryName = string.Join(" ", category.Key.Split('_'));
            Console.WriteLine(coolColors.Colorize($"{categoryName}:", "cyan"));
            foreach (var command in category.Value)
            {
                Console.WriteLine();
                Console.WriteLine(coolColors.Colorize($"{command.Command}", "cyan"));
                Console.WriteLine(coolColors.Colorize($" - {command.Description}", "cyan"));
                Console.WriteLine(coolColors.Colorize($" - Requires Custom Libraries: <{command.CustomLibs}>", "cyan"));
            }
            Console.WriteLine(coolColors.Colorize($"-------------------------", "blue"));
        }
    }
}
