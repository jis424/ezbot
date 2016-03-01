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
using System.Diagnostics;

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
        public static string LoLVersion = "";// = "6.4.16_02_22_17_11";
        public static bool buyExpBoost = false;
        public static bool checkUpdates = true;

        private static WebClient client = null;

        private static void Main(string[] args)
        {
          /*  Application.EnableVisualStyles();
            donate wwForm = new donate();
            wwForm.ShowDialog();
            */

            Console.Title = "ezBot";
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);

            Tools.TitleMessage("ezBot - Auto Queue.");
            Tools.TitleMessage("Version: " + Application.ProductVersion);
            Tools.TitleMessage("Made by Tryller.");
            Tools.TitleMessage("Based on VoliBot.");

            Tools.ConsoleMessage("Loading config.");
            loadConfiguration();

            client = new WebClient();
            if (checkUpdates)
            {
                try
                {
                    int num = int.Parse(Tools.ezVersion.ToString().Replace(".", ""));
                    int num2 = 0;
                    try
                    {
                        Stream stream = client.OpenRead(Tools.versionURL);
                        num2 = int.Parse(new StreamReader(stream).ReadLine());
                        stream.Close();
                    }
                    catch (Exception ex)
                    {
                        Tools.ConsoleMessage(ex.Message.ToString());
                    }

                    if (num < num2)
                    {
                        MessageBox.Show("A new version of ezBot was released.\nPlease goto EloBuddy forums and download latest version.", "ezBot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
                catch
                {
                    MessageBox.Show("Could not check the update.", "ezBot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }

            if (replaceConfig)
            {
                Tools.ConsoleMessage("Replacing config.");
                Tools.ReplaceGameConfig(lolPath);
            }

            Tools.ConsoleMessage("Loading accounts.");
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
                    ezBot ezBot;
                    if (result[2] != null)
                    {
                        queuetype = (QueueTypes)System.Enum.Parse(typeof(QueueTypes), result[2]);
                        ezBot = new ezBot(result[0], result[1], Region, lolPath, queuetype, LoLVersion);
                    }
                    else
                    {
                        queuetype = QueueTypes.ARAM;
                        ezBot = new ezBot(result[0], result[1], Region, lolPath, queuetype, LoLVersion);
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
                    ezBot ritoBot = new ezBot(result[0], result[1], Region, lolPath, queuetype, LoLVersion);
                }
                else
                {
                    QueueTypes queuetype = QueueTypes.ARAM;
                    ezBot ritoBot = new ezBot(result[0], result[1], Region, lolPath, queuetype, LoLVersion);
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
                checkUpdates = Convert.ToBoolean(iniFile.IniReadValue("General", "CheckUpdates"));
                lolPath = iniFile.IniReadValue("General", "LauncherPath");
                maxBots = Convert.ToInt32(iniFile.IniReadValue("General", "MaxBots"));
                maxLevel = Convert.ToInt32(iniFile.IniReadValue("General", "MaxLevel"));
                replaceConfig = Convert.ToBoolean(iniFile.IniReadValue("General", "ReplaceLoLConfig"));
                LoLVersion = iniFile.IniReadValue("General", "LoLVersion");

                //Account
                Region = iniFile.IniReadValue("Account", "Region").ToUpper();
                buyExpBoost = Convert.ToBoolean(iniFile.IniReadValue("Account", "BuyExpBoost"));

                //LOL
                firstChampionPick = iniFile.IniReadValue("LOL", "ChampionPick").ToUpper();
                secondChampionPick = iniFile.IniReadValue("LOL", "SecondChampionPick").ToUpper();
                randomSpell = Convert.ToBoolean(iniFile.IniReadValue("LOL", "RandomSpell"));
                spell1 = iniFile.IniReadValue("LOL", "Spell1").ToUpper();
                spell2 = iniFile.IniReadValue("LOL", "Spell2").ToUpper();
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
