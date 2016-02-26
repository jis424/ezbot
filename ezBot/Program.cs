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

namespace ezBot
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
        public static bool buyExpBoost = false;

        private static void Main(string[] args)
        {
            Console.Title = "ezBot";
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);

            Tools.TitleMessage("ezBot - Auto Queue.");
            Tools.TitleMessage("Version: " + Application.ProductVersion);
            Tools.TitleMessage("Made by Tryller");

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

                    QueueTypes queuetype;
                    ezBot ritoBot;
                    if (result[2] != null)
                    {
                        queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[2]);
                        ritoBot = new ezBot(result[0], result[1], Region, lolPath, curRunning, queuetype, LoLVersion);
                    }
                    else
                    {
                        queuetype = QueueTypes.ARAM;
                        ritoBot = new ezBot(result[0], result[1], Region, lolPath, curRunning, queuetype, LoLVersion);
                    }

                    if (curRunning == maxBots)
                        break;
                }
                catch (Exception)
                {
                    Tools.ConsoleMessage("You may have an issue in your accounts.txt");
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
                if (result[2] != null)
                {
                    QueueTypes queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[2]);
                    ezBot ritoBot = new ezBot(result[0], result[1], Region, lolPath, curRunning, queuetype, LoLVersion);
                }
                else
                {
                    QueueTypes queuetype = QueueTypes.ARAM;
                    ezBot ritoBot = new ezBot(result[0], result[1], Region, lolPath, curRunning, queuetype, LoLVersion);
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
                buyExpBoost = Convert.ToBoolean(iniFile.IniReadValue("Account", "BuyExpBoost"));

                //LOL
                firstChampionPick = iniFile.IniReadValue("LOL", "ChampionPick").ToUpper();
                secondChampionPick = iniFile.IniReadValue("LOL", "SecondChampionPick").ToUpper();
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