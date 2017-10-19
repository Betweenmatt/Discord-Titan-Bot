using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titan_Bot
{
    public static class GlobalProperties
    {
        /// <summary>
        /// the list  of players currently in battle
        /// </summary>
        public static List<string> inBattle = new List<string>();
        /// <summary>
        /// the list of titans schedule, including reservations. pulled from txt file and placed
        /// in this object for easy reference
        /// </summary>
        public static List<TitanObj> schedule = new List<TitanObj>();
        /// <summary>
        /// the current titan level, puled from settings.txt
        /// </summary>
        public static int currentLevel = 0;
        /// <summary>
        /// the current auto-delete flag, pulled from settings.txt
        /// </summary>
        public static bool deleteCommands = false;
        public static List<string> mutedList = new List<string>();
        public static string currentTitanHp = "Not Set";

        public static List<LmkObject> lmkList = new List<LmkObject>();


        public static DiscordClient discord;
        public static CommandsNextModule commands;
        public static Dictionary<int, string> titanHpList = new Dictionary<int, string>();
        public static DiscordChannel ReadOnlyChannel = null;
        public static DiscordChannel CommandChannel = null;
        public static DiscordChannel GeneralChannel = null;
        public static ulong ReadOnlyId;
        public static ulong CommandChannelId;
        public static ulong GeneralChannelId;

        public static string AfternoonStart = "13:00";
        public static string AfternoonEnd = "17:00";
        public static string EveningStart = "21:00";
        public static string EveningEnd = "24:00";

        public static List<string> QuietModeList = new List<string>();
        public static List<string> OfficerList = new List<string>();
        
        public static string Token = "";
        //determines if the bot needs to be setup or not.
        public static bool IsSetup;

#if !DEBUG
        public static DiscordGame thisGame = new DiscordGame("Monster Super League!");
        public static DiscordGame waitingGame = new DiscordGame("The Waiting Game...");
#else
        public static DiscordGame thisGame = new DiscordGame("Debug Battle Current");
        public static DiscordGame waitingGame = new DiscordGame("Debug Waiting Battle");
#endif
    }
}
