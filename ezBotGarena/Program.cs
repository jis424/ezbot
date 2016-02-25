using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;
using System.Net;
using System.Management;
using LoLLauncher;
using System.Windows.Forms;

namespace ezBotGarena
{
    class Program
    {
        public static string lolPath;
        public static string Region;
        public static ArrayList accounts = new ArrayList();
        public static ArrayList accountsNew = new ArrayList();
        public static int maxBots = 1;
        public static bool replaceConfig = false;
        public static string firstChampionPick = "Ashe";
        public static string secondChampionPick = "Sivir";
        public static int maxLevel = 30;
        public static bool randomSpell = false;
        public static string spell1 = "flash";
        public static string spell2 = "ignite";
        public static string LoLVersion = "";// = "6.3.16_02_05_12_04";

        private static void Main(string[] args)
        {
            Console.Title = "ezBot Garena";
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);

            Tools.ConsoleMessage("ezBot - Auto Queue for Garena.");
            Tools.ConsoleMessage("Version: " + Application.ProductVersion);
            Tools.ConsoleMessage("Made by Tryller");
            Console.Write("\n");

            Tools.ConsoleMessage("Loading config");
            loadConfiguration();

            if (replaceConfig)
            {
                Tools.ConsoleMessage("Replacing config");
                Tools.ReplaceGameConfig(lolPath);
            }

            Tools.ConsoleMessage("Loading accounts");
            loadAccounts();
            int curRunning = 0;
            foreach (string acc in accounts)
            {
                try
                {
                    accountsNew.RemoveAt(0);
                    string Accs = acc;
                    string[] stringSeparators = new string[] { "|" };
                    var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                    curRunning += 1;
                    string token = "";

                    token = Tools.GetGarenaToken();
                    if (result[1] != null)
                    {
                        QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[1]);
                        ezBotGarena ritoBot = new ezBotGarena(result[0], token, Region, lolPath, curRunning, queuetype, LoLVersion);
                    }
                    else
                    {
                        QueueTypes queuetype = QueueTypes.ARAM;
                        ezBotGarena ritoBot = new ezBotGarena(result[0], token, Region, lolPath, curRunning, queuetype, LoLVersion);
                    }

                    if (curRunning == maxBots)
                        break;
                }
                catch (Exception)
                {
                    Tools.ConsoleMessage("You may have an issue in your configs\accounts.txt");
                }
            }
            Console.ReadKey();
        }

        public static void lognNewAccount()
        {
            accountsNew = accounts;
            accounts.RemoveAt(0);
            int curRunning = 0;
            if (accounts.Count == 0)
            {
                Tools.ConsoleMessage("No more acocunts to login");
            }
            foreach (string acc in accounts)
            {
                string Accs = acc;
                string[] stringSeparators = new string[] {"|"};
                var result = Accs.Split(stringSeparators, StringSplitOptions.None);
                curRunning += 1;
                string token = "";

                if (result[1] != null)
                {
                    QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[1]);
                    ezBotGarena ritoBot = new ezBotGarena(result[0], token, Region, lolPath, curRunning, queuetype, LoLVersion);
                }
                else
                {
                    QueueTypes queuetype = QueueTypes.ARAM;
                    ezBotGarena ritoBot = new ezBotGarena(result[0], token, Region, lolPath, curRunning, queuetype, LoLVersion);
                }

                if (curRunning == maxBots)
                    break;
            }
        }
        public static void loadConfiguration()
        {
            try
            {
                IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "\\configs\\settings.ini");

                //General
                lolPath = iniFile.IniReadValue("General", "LauncherPath");
                maxBots = Convert.ToInt32(iniFile.IniReadValue("General", "MaxBots"));
                maxLevel = Convert.ToInt32(iniFile.IniReadValue("General", "MaxLevel"));
                replaceConfig = Convert.ToBoolean(iniFile.IniReadValue("General", "ReplaceLoLConfig"));

                //Account
                Region = iniFile.IniReadValue("Account", "Region").ToUpper();

                //LOL
                firstChampionPick = iniFile.IniReadValue("LOL", "ChampionPick").ToUpper();
                firstChampionPick = iniFile.IniReadValue("LOL", "SecondChampionPick").ToUpper();
                randomSpell = Convert.ToBoolean(iniFile.IniReadValue("LOL", "RndSpell"));
                spell1 = iniFile.IniReadValue("LOL", "Spell1").ToUpper();
                spell2 = iniFile.IniReadValue("LOL", "Spell2").ToUpper();
                LoLVersion = iniFile.IniReadValue("LOL", "LoLVersion");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(10000);
                Application.Exit();
            }
        }

        public static void loadAccounts()
        {
            TextReader tr = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\configs\\accounts.txt");
            string line;
            while ((line = tr.ReadLine()) != null)
            {
                accounts.Add(line);
                accountsNew.Add(line);
            }
            tr.Close();
        }
    }
}