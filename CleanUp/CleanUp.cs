using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CleanUp
{
    public class CleanUp
    {

        private readonly string SteamDefault = @"C:\Program Files (x86)\Steam\steamapps";
        private readonly string DistribPath = @"\_CommonRedist\vcredist";
        private string _steamFolder;

        public CleanUp(string steam = null)
        {
            _steamFolder = steam ?? SteamDefault;
        }

        public void Start()
        {
            var installedProgs = GetProgramList();
            var games = GetSteamList();
            Console.WriteLine("Games found: {0}", games.Count);
            foreach (var game in games)
            {
                game.GetDistributableInfo(DistribPath);
            }
            
        }

        private List<InstalledProgram> GetProgramList()
        {
            var installRegLoc = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            var hkey = (Registry.LocalMachine).OpenSubKey(installRegLoc, true);
            var newList = hkey.GetSubKeyNames();
            Console.WriteLine("Pre-filter count: " + newList.Length);
            installRegLoc += @"\";
            var programList = newList.Where(s => (Registry.LocalMachine).OpenSubKey(installRegLoc + s, true).GetValue("DisplayName") != null).Select(s => (Registry.LocalMachine).OpenSubKey(installRegLoc + s, true)).ToList();
            Console.WriteLine("Post-filter count: " + programList.Count);
            var installedList = (from s in programList
                let programName = s.GetValue("DisplayName")
                where programName != null
                select new InstalledProgram
                {
                    Name = programName.ToString(), Version = s.GetValue("DisplayVersion")?.ToString(), RegistryKey = s
                }).ToList();
            return installedList;
        }

        private List<Game> GetSteamList()
        {
            //start by getting a breakdown of each program in the steam folder based on the folder name
            var common = _steamFolder + @"\common";
            var games = new List<string>(Directory.EnumerateDirectories(common));
            var gameList = (from game in games where HasDistributable(game) select new Game {Name = game.Substring(game.LastIndexOf(@"\") + 1), Location = game}).ToList();
            return gameList;
        }

        private bool HasDistributable(string location)
        {
            var distrubFolder = location + DistribPath;
            if (Directory.Exists(distrubFolder))
            {
                return true;
            }
            return false;
        }



    }
    public class InstalledProgram
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public RegistryKey RegistryKey { get; set; }
    }

    public class Game
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public List<Distributable> Distributables { get; set; }
    }

    public class Distributable
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Location { get; set; }
    }

    public static class Helper
    {
        public static void GetDistributableInfo(this Game game, string distribPath)
        {
            var distribFolders = new List<string>(Directory.EnumerateDirectories(game.Location + distribPath));
            game.Distributables = new List<Distributable>();
            foreach (var folder in distribFolders)
            {
                var fileName = "vcredist_x86.exe";
                var filePath = Path.Combine(folder, fileName);
                if (File.Exists(filePath))
                {
                    var distrib = new Distributable
                    {
                        Location = filePath,
                        Version = FileVersionInfo.GetVersionInfo(filePath).ProductVersion,
                        Name = FileVersionInfo.GetVersionInfo(filePath).ProductName
                    };
                    game.Distributables.Add(distrib);
                }

                if (Environment.Is64BitOperatingSystem)
                {
                    fileName = "vcredist_x64.exe";
                    filePath = Path.Combine(folder, fileName);
                    if (!File.Exists(filePath)) continue;
                    var distrib = new Distributable
                    {
                        Location = filePath,
                        Version = FileVersionInfo.GetVersionInfo(filePath).ProductVersion,
                        Name = FileVersionInfo.GetVersionInfo(filePath).ProductName
                    };
                    game.Distributables.Add(distrib);
                }
            }
        }
    }
}
