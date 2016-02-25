using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using LoLLauncher;
using LoLLauncher.RiotObjects;
using LoLLauncher.RiotObjects.Platform.Catalog.Champion;
using LoLLauncher.RiotObjects.Platform.Clientfacade.Domain;
using LoLLauncher.RiotObjects.Platform.Game;
using LoLLauncher.RiotObjects.Platform.Game.Message;
using LoLLauncher.RiotObjects.Platform.Matchmaking;
using LoLLauncher.RiotObjects.Platform.Statistics;
using LoLLauncher.RiotObjects.Leagues.Pojo;
using LoLLauncher.RiotObjects.Platform.Game.Practice;
using LoLLauncher.RiotObjects.Platform.Harassment;
using LoLLauncher.RiotObjects.Platform.Leagues.Client.Dto;
using LoLLauncher.RiotObjects.Platform.Login;
using LoLLauncher.RiotObjects.Platform.Reroll.Pojo;
using LoLLauncher.RiotObjects.Platform.Statistics.Team;
using LoLLauncher.RiotObjects.Platform.Summoner;
using LoLLauncher.RiotObjects.Platform.Summoner.Boost;
using LoLLauncher.RiotObjects.Platform.Summoner.Masterybook;
using LoLLauncher.RiotObjects.Platform.Summoner.Runes;
using LoLLauncher.RiotObjects.Platform.Summoner.Spellbook;
using LoLLauncher.RiotObjects.Team;
using LoLLauncher.RiotObjects.Team.Dto;
using LoLLauncher.RiotObjects.Platform.Game.Map;
using LoLLauncher.RiotObjects.Platform.Summoner.Icon;
using LoLLauncher.RiotObjects.Platform.Catalog.Icon;
using LoLLauncher.RiotObjects.Platform.Messaging;

namespace ezBot
{
    internal class ezBot
    {
        public LoginDataPacket loginPacket = new LoginDataPacket();
        public GameDTO currentGame = new GameDTO();
        public LoLConnection connection = new LoLConnection();
        public List<ChampionDTO> availableChamps = new List<ChampionDTO>();
        public ChampionDTO[] availableChampsArray;
        public bool firstTimeInLobby = true;
        public bool firstTimeInQueuePop = true;
        public bool firstTimeInCustom = true;
        public Process exeProcess;
        public string ipath;
        public string Accountname;
        public string Password;
        public int threadID;
        public double sumLevel { get; set; }
        public double archiveSumLevel { get; set; }
        public double rpBalance { get; set; }
        public QueueTypes queueType { get; set; }
        public QueueTypes actualQueueType { get; set; }

        public bool firstTimeInPostChampSelect = true;

        public string region { get; set; }

        //leave buster
        public int relogTry = 0;
        public int m_leaverBustedPenalty { get; set; }
        public string m_accessToken { get; set; }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public ezBot(string username, string password, string reg, string path, int threadid, QueueTypes QueueType, string LoLVersion)
        {
            ipath = path;
            Accountname = username;
            Password = password;
            threadID = threadid;
            queueType = QueueType;
            region = reg;
            connection.OnConnect += new LoLConnection.OnConnectHandler(this.connection_OnConnect);
            connection.OnDisconnect += new LoLConnection.OnDisconnectHandler(this.connection_OnDisconnect);
            connection.OnError += new LoLConnection.OnErrorHandler(this.connection_OnError);
            connection.OnLogin += new LoLConnection.OnLoginHandler(this.connection_OnLogin);
            connection.OnLoginQueueUpdate += new LoLConnection.OnLoginQueueUpdateHandler(this.connection_OnLoginQueueUpdate);
            connection.OnMessageReceived += new LoLConnection.OnMessageReceivedHandler(this.connection_OnMessageReceived);
            switch (region)
            {
                case "EUW":
                    connection.Connect(username, password, Region.EUW, LoLVersion);
                    break;
                case "EUNE":
                    connection.Connect(username, password, Region.EUN, LoLVersion);
                    break;
                case "NA":
                    connection.Connect(username, password, Region.NA, LoLVersion);
                    break;
                case "KR":
                    connection.Connect(username, password, Region.KR, LoLVersion);
                    break;
                case "BR":
                    connection.Connect(username, password, Region.BR, LoLVersion);
                    break;
                case "OCE":
                    connection.Connect(username, password, Region.OCE, LoLVersion);
                    break;
                case "RU":
                    connection.Connect(username, password, Region.RU, LoLVersion);
                    break;
                case "TR":
                    connection.Connect(username, password, Region.TR, LoLVersion);
                    break;
                case "LAS":
                    connection.Connect(username, password, Region.LAS, LoLVersion);
                    break;
                case "LAN":
                    connection.Connect(username, password, Region.LAN, LoLVersion);
                    break;
            }
        }

        public async void connection_OnMessageReceived(object sender, object message)
        {
            if (message is GameDTO)
            {
                GameDTO game = message as GameDTO;
                switch (game.GameState)
                {
                    case "START_REQUESTED":
                        break;
                    case "FAILED_TO_START":
                        Tools.ConsoleMessage("Failed to Start");
                        break;
                    case "CHAMP_SELECT":
                        firstTimeInCustom = true;
                        firstTimeInQueuePop = true;
                        if (firstTimeInLobby)
                        {
                            firstTimeInLobby = false;
                            Tools.ConsoleMessage("You are in champion select.");
                            object obj = await connection.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");
                            if (queueType != QueueTypes.ARAM)
                            {
                                //Select frst champion
                                if (Program.firstChampionPick != "" && Program.firstChampionPick != "RANDOM")
                                {

                                    int Spell1;
                                    int Spell2;
                                    if (!Program.randomSpell)
                                    {
                                        Spell1 = Enums.spellToId(Program.spell1);
                                        Spell2 = Enums.spellToId(Program.spell2);
                                    }
                                    else
                                    {
                                        var random = new Random();
                                        var spellList = new List<int> { 13, 6, 7, 10, 1, 11, 21, 12, 3, 14, 2, 4 };

                                        int index = random.Next(spellList.Count);
                                        int index2 = random.Next(spellList.Count);

                                        int randomSpell1 = spellList[index];
                                        int randomSpell2 = spellList[index2];

                                        if (randomSpell1 == randomSpell2)
                                        {
                                            int index3 = random.Next(spellList.Count);
                                            randomSpell2 = spellList[index3];
                                        }

                                        Spell1 = Convert.ToInt32(randomSpell1);
                                        Spell2 = Convert.ToInt32(randomSpell2);
                                    }

                                    await connection.SelectSpells(Spell1, Spell2);

                                    await connection.SelectChampion(Enums.championToId(Program.firstChampionPick));
                                    await connection.ChampionSelectCompleted();
                                }
                                else
                                {

                                    int Spell1;
                                    int Spell2;
                                    if (!Program.randomSpell)
                                    {
                                        Spell1 = Enums.spellToId(Program.spell1);
                                        Spell2 = Enums.spellToId(Program.spell2);
                                    }
                                    else
                                    {
                                        var random = new Random();
                                        var spellList = new List<int> { 13, 6, 7, 10, 1, 11, 21, 12, 3, 14, 2, 4 };

                                        int index = random.Next(spellList.Count);
                                        int index2 = random.Next(spellList.Count);

                                        int randomSpell1 = spellList[index];
                                        int randomSpell2 = spellList[index2];

                                        if (randomSpell1 == randomSpell2)
                                        {
                                            int index3 = random.Next(spellList.Count);
                                            randomSpell2 = spellList[index3];
                                        }

                                        Spell1 = Convert.ToInt32(randomSpell1);
                                        Spell2 = Convert.ToInt32(randomSpell2);
                                    }

                                    await connection.SelectSpells(Spell1, Spell2);

                                    var randAvailableChampsArray = availableChampsArray.Shuffle();
                                    await connection.SelectChampion(Enums.championToId(Program.secondChampionPick));
                                    await connection.ChampionSelectCompleted();
                                }
                            }
                            break;
                        }
                        else
                            break;
                    case "POST_CHAMP_SELECT":
                        firstTimeInLobby = false;
                        if (firstTimeInPostChampSelect)
                        {
                            firstTimeInPostChampSelect = false;
                            Tools.ConsoleMessage("Waiting for league of legends.");
                        }
                        break;
                    case "IN_QUEUE":
                        Tools.ConsoleMessage("You are in queue.");
                        break;
                    case "TERMINATED":
                        Tools.ConsoleMessage("Re-entering queue.");
                        firstTimeInPostChampSelect = true;
                        firstTimeInQueuePop = true;
                        break;
                    case "JOINING_CHAMP_SELECT":
                        if (firstTimeInQueuePop && game.StatusOfParticipants.Contains("1"))
                        {
                            Tools.ConsoleMessage("Queue found and accepted.");
                            firstTimeInQueuePop = false;
                            firstTimeInLobby = true;
                            object obj = await connection.AcceptPoppedGame(true);
                            break;
                        }
                        else
                            break;
                    case "LEAVER_BUSTED":
                        Tools.ConsoleMessage("You have leave buster.");
                        break;
                    default:
                        Tools.ConsoleMessage("[DEFAULT]" + game.GameStateString);
                        break;
                }
            }
            else if (message is PlayerCredentialsDto)
            {
                firstTimeInPostChampSelect = true;
                PlayerCredentialsDto dto = message as PlayerCredentialsDto;
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = FindLoLExe();
                startInfo.FileName = "League of Legends.exe";
                startInfo.Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"" + dto.ServerIp + " " +
                    dto.ServerPort + " " + dto.EncryptionKey + " " + dto.SummonerId + "\"";
                Tools.ConsoleMessage("Launching League of Legends\n");

                new Thread(() =>
                {
                    exeProcess = Process.Start(startInfo);
                    exeProcess.Exited += new EventHandler(exeProcess_Exited);
                    while (exeProcess.MainWindowHandle == IntPtr.Zero) { }
                    exeProcess.PriorityClass = ProcessPriorityClass.Idle;
                    exeProcess.EnableRaisingEvents = true;
                    //Thread.Sleep(1000);
                }).Start();
            }
            else if (!(message is GameNotification) && !(message is SearchingForMatchNotification))
            {
                if (message is EndOfGameStats)
                {
                    AttachToQueue();
                }
                else
                {
                    if (message.ToString().Contains("EndOfGameStats"))
                    {
                        /*EndOfGameStats eog = new EndOfGameStats();
                        exeProcess.Exited -= exeProcess_Exited;
                        exeProcess.Kill();*/
                        //Process lol = Process.GetProcessById(Tools.GetLOLProcessID());
                        foreach (Process process in Process.GetProcessesByName("League Of Legends"))
                        {
                            if (process.Id == Tools.GetLOLProcessID())
                            {
                                process.Kill();
                            }
                        }

                        loginPacket = await this.connection.GetLoginDataPacketForUser();
                        archiveSumLevel = sumLevel;
                        sumLevel = loginPacket.AllSummonerData.SummonerLevel.Level;
                        if (sumLevel != archiveSumLevel)
                        {
                            levelUp();
                        }

                        AttachToQueue();
                    }
                }
            }
        }

        private async void AttachToQueue()
        {

            MatchMakerParams matchParams = new MatchMakerParams();
            //Set BotParams
            if (queueType == QueueTypes.INTRO_BOT)
            {
                matchParams.BotDifficulty = "INTRO";
            }
            else if (queueType == QueueTypes.BEGINNER_BOT)
            {
                matchParams.BotDifficulty = "EASY";
            }
            else if (queueType == QueueTypes.MEDIUM_BOT)
            {
                matchParams.BotDifficulty = "MEDIUM";
            }
            //Check if is available to join queue.
            if (sumLevel == 3 && actualQueueType == QueueTypes.NORMAL_5x5)
            {
                queueType = actualQueueType;
            }
            else if (sumLevel == 6 && actualQueueType == QueueTypes.ARAM)
            {
                queueType = actualQueueType;
            }
            else if (sumLevel == 7 && actualQueueType == QueueTypes.NORMAL_3x3)
            {
                queueType = actualQueueType;
            }
            matchParams.QueueIds = new Int32[1] { (int)queueType };
            SearchingForMatchNotification m = await connection.AttachToQueue(matchParams);

            if (m.PlayerJoinFailures == null)
            {
                Tools.ConsoleMessage("In Queue: " + queueType.ToString());
            }
            else
            {
                foreach (var failure in m.PlayerJoinFailures)
                {
                    if (failure.ReasonFailed == "LEAVER_BUSTED")
                    {
                        m_accessToken = failure.AccessToken;
                        if (failure.LeaverPenaltyMillisRemaining > m_leaverBustedPenalty)
                        {
                            m_leaverBustedPenalty = failure.LeaverPenaltyMillisRemaining;
                        }
                    }
                    else if (failure.ReasonFailed == "LEAVER_BUSTER_TAINTED_WARNING")
                    {
                        //updateStatus("Please login on your LoL Client and type I Agree to the message that comes up.", Accountname);
                        await connection.ackLeaverBusterWarning();
                        await connection.callPersistenceMessaging(new SimpleDialogMessageResponse()
                        {
                            AccountID = loginPacket.AllSummonerData.Summoner.SumId,
                            MsgID = loginPacket.AllSummonerData.Summoner.SumId,
                            Command = "ack"
                        });
                        connection_OnMessageReceived(null, (object)new EndOfGameStats());
                    }
                }

                if (String.IsNullOrEmpty(m_accessToken))
                {
                    // Queue dodger or something else
                }
                else
                {
                    Tools.ConsoleMessage("Waiting leaver buster time: " + m_leaverBustedPenalty / 1000 / (float)60 + " minutes!");
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(m_leaverBustedPenalty));
                    m = await connection.AttachToLowPriorityQueue(matchParams, m_accessToken);
                    if (m.PlayerJoinFailures == null)
                    {
                        Tools.ConsoleMessage("Joined lower priority queue!");
                    }
                    else
                    {
                        Tools.ConsoleMessage("There was an error in joining lower priority queue.\nDisconnecting.");
                        connection.Disconnect();
                        Program.lognNewAccount();
                    }
                }
            }
        }

        private String FindLoLExe()
        {
            String installPath = ipath;
            if (installPath.Contains("notfound"))
                return installPath;
            installPath += @"RADS\solutions\lol_game_client_sln\releases\";
            installPath = Directory.EnumerateDirectories(installPath).OrderBy(f => new DirectoryInfo(f).CreationTime).Last();
            installPath += @"\deploy\";
            return installPath;
        }

        async void exeProcess_Exited(object sender, EventArgs e)
        {
            Tools.ConsoleMessage("Restart League of Legends.");
            loginPacket = await connection.GetLoginDataPacketForUser();
            if (this.loginPacket.ReconnectInfo != null && this.loginPacket.ReconnectInfo.Game != null)
            {
                this.connection_OnMessageReceived(sender, (object)this.loginPacket.ReconnectInfo.PlayerCredentials);
            }
            else
                this.connection_OnMessageReceived(sender, (object)new EndOfGameStats());
        }

        private async void RegisterNotifications()
        {
            object obj1 = await this.connection.Subscribe("bc", this.connection.AccountID());
            object obj2 = await this.connection.Subscribe("cn", this.connection.AccountID());
            object obj3 = await this.connection.Subscribe("gn", this.connection.AccountID());
        }

        private void connection_OnLoginQueueUpdate(object sender, int positionInLine)
        {
            if (positionInLine <= 0)
                return;

            Tools.ConsoleMessage("Position to login: " + (object)positionInLine);
        }

        private void connection_OnLogin(object sender, string username, string ipAddress)
        {
            new Thread((ThreadStart)(async () =>
            {
                Tools.ConsoleMessage("Connecting...");
                this.RegisterNotifications();
                this.loginPacket = await this.connection.GetLoginDataPacketForUser();
                if (loginPacket.AllSummonerData == null)
                {
                    Random rnd = new Random();
                    String summonerName = Accountname;
                    if (summonerName.Length > 16)
                        summonerName = summonerName.Substring(0, 12) + new Random().Next(1000, 9999).ToString();
                    AllSummonerData sumData = await connection.CreateDefaultSummoner(summonerName);
                    loginPacket.AllSummonerData = sumData;
                    Tools.ConsoleMessage("Created Summonername " + summonerName);
                }
                sumLevel = loginPacket.AllSummonerData.SummonerLevel.Level;
                string sumName = loginPacket.AllSummonerData.Summoner.Name;
                double sumId = loginPacket.AllSummonerData.Summoner.SumId;
                rpBalance = loginPacket.RpBalance;
                if (sumLevel >= Program.maxLevel)
                {
                    connection.Disconnect();
                    Tools.ConsoleMessage("Summoner: " + sumName + " is already max level.");
                    Tools.ConsoleMessage("Log into new account.");
                    Program.lognNewAccount();
                    return;
                }

                if (sumLevel < 3.0 && queueType == QueueTypes.NORMAL_5x5)
                {
                    Tools.ConsoleMessage("Need to be Level 3 before NORMAL_5x5 queue.");
                    Tools.ConsoleMessage("Joins Co-Op vs AI (Beginner) queue until 3");
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.NORMAL_5x5;
                }
                else if (sumLevel < 6.0 && queueType == QueueTypes.ARAM)
                {
                    Tools.ConsoleMessage("Need to be Level 6 before ARAM queue.");
                    Tools.ConsoleMessage("Joins Co-Op vs AI (Beginner) queue until 6");
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.ARAM;
                }
                else if (sumLevel < 7.0 && queueType == QueueTypes.NORMAL_3x3)
                {
                    Tools.ConsoleMessage("Need to be Level 7 before NORMAL_3x3 queue.");
                    Tools.ConsoleMessage("Joins Co-Op vs AI (Beginner) queue until 7");
                    queueType = QueueTypes.BEGINNER_BOT;
                    actualQueueType = QueueTypes.NORMAL_3x3;
                }

                Tools.ConsoleMessage("Welcome to the League " + loginPacket.AllSummonerData.Summoner.Name);
                availableChampsArray = await connection.GetAvailableChampions();
                PlayerDTO player = await connection.CreatePlayer();
                if (this.loginPacket.ReconnectInfo != null && this.loginPacket.ReconnectInfo.Game != null)
                {
                    this.connection_OnMessageReceived(sender, (object)this.loginPacket.ReconnectInfo.PlayerCredentials);
                }
                else
                    this.connection_OnMessageReceived(sender, (object)new EndOfGameStats());
            })).Start();
        }

        private void connection_OnError(object sender, LoLLauncher.Error error)
        {
            if (error.Message.Contains("is not owned by summoner"))
            {
                return;
            }
            else if (error.Message.Contains("Your summoner level is too low to select the spell"))
            {
                var random = new Random();
                var spellList = new List<int> { 13, 6, 7, 10, 1, 11, 21, 12, 3, 14, 2, 4 };

                int index = random.Next(spellList.Count);
                int index2 = random.Next(spellList.Count);

                int randomSpell1 = spellList[index];
                int randomSpell2 = spellList[index2];

                if (randomSpell1 == randomSpell2)
                {
                    int index3 = random.Next(spellList.Count);
                    randomSpell2 = spellList[index3];
                }

                int Spell1 = Convert.ToInt32(randomSpell1);
                int Spell2 = Convert.ToInt32(randomSpell2);
                return;
            }

            Tools.ConsoleMessage("error received:\n" + error.Message);
        }

        private void connection_OnDisconnect(object sender, EventArgs e)
        {
            Console.Title = "ezBot - Offline";
            Tools.ConsoleMessage("Disconnected");
        }

        private void connection_OnConnect(object sender, EventArgs e)
        {
            Console.Title = "ezBot - Online";
        }

        public void levelUp()
        {
            Tools.ConsoleMessage("Level Up: " + sumLevel);
            rpBalance = loginPacket.RpBalance;
            if (sumLevel >= Program.maxLevel)
            {
                connection.Disconnect();
                //bool connectStatus = await connection.IsConnected();
                if (!connection.IsConnected())
                {
                    Program.lognNewAccount();
                }
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            List<T> buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
