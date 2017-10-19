using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titan_Bot
{
    public enum TimeFrame { MorningWait, MorningBattle, AfternoonWait, AfternoonBattle }
    public class Utils
    {

        /// <summary>
        /// Returns the current timeframe 
        /// </summary>
        /// <returns></returns>
        public static BattleTime GetCurrentTimeFrame()
        {
            try { 
                DateTime now = DateTime.Now;
                DateTime afternoonstart = Convert.ToDateTime(GlobalProperties.AfternoonStart);
                DateTime eveningstart = Convert.ToDateTime(GlobalProperties.EveningStart);
                DateTime afternoonend = Convert.ToDateTime(GlobalProperties.AfternoonEnd);
                DateTime eveningend = Convert.ToDateTime(GlobalProperties.EveningEnd);

                if (now > afternoonstart && now < afternoonend)//first battle active
                {
                    TimeSpan x = afternoonend - now;
                    string timeleft = x.Hours.ToString() + ":" + x.Minutes.ToString() + ":" + x.Seconds.ToString();
                    return new BattleTime() {
                        TimeFrame = TimeFrame.MorningBattle,
                        Time = timeleft
                    };
                }
                else if (now > eveningstart && now < eveningend)//second battle active
                {
                    TimeSpan x = eveningend - now;
                    string timeleft = x.Hours.ToString() + ":" + x.Minutes.ToString() + ":" + x.Seconds.ToString();
                    return new BattleTime()
                    {
                        TimeFrame = TimeFrame.AfternoonBattle,
                        Time = timeleft
                    };
                }
                else if (now > afternoonend && now < eveningstart)//in between first and second
                {
                    TimeSpan x = eveningstart - now;
                    string timeleft = x.Hours.ToString() + ":" + x.Minutes.ToString() + ":" + x.Seconds.ToString();
                    return new BattleTime()
                    {
                        TimeFrame = TimeFrame.AfternoonWait,
                        Time = timeleft
                    };
                }
                else//after last
                {
                    TimeSpan x = afternoonstart - now;
                    string timeleft = x.Hours.ToString() + ":" + x.Minutes.ToString() + ":" + x.Seconds.ToString();
                    return new BattleTime()
                    {
                        TimeFrame = TimeFrame.MorningWait,
                        Time = timeleft
                    };
                }
            }
            catch (Exception e)
            {
                FileHandler.SaveToLog(e.ToString());
            }
            return new BattleTime()
            {
                TimeFrame = TimeFrame.MorningWait,
                Time = ""
            };
        }

        /// <summary>
        /// resets the list of titans
        /// </summary>
        public static void ResetTitanSchedule()
        {
            Console.WriteLine("Overwriting titan schedule");
            int elementindex = 0;
            GlobalProperties.schedule.Clear();
            for (var i = 0; i < 100; i++)
            {
                //check list for hp
                string temp = "";
                GlobalProperties.titanHpList.TryGetValue(i + 1, out temp);
                string hp = (temp == null || temp == "") ? "???" : temp.ToString() + "M";

                if (elementindex == 0)
                {

                    GlobalProperties.schedule.Add(new TitanObj
                    {
                        element = $"Dark {hp}",
                        level = i + 1
                    });
                    elementindex++;
                }
                else if (elementindex == 1)
                {
                    GlobalProperties.schedule.Add(new TitanObj
                    {
                        element = $"Water {hp}",
                        level = i + 1
                    });
                    elementindex++;
                }
                else if (elementindex == 2)
                {
                    GlobalProperties.schedule.Add(new TitanObj
                    {
                        element = $"Wood {hp}",
                        level = i + 1
                    });
                    elementindex++;
                }
                else if (elementindex == 3)
                {
                    GlobalProperties.schedule.Add(new TitanObj
                    {
                        element = $"Light {hp}",
                        level = i + 1
                    });
                    elementindex++;
                }
                else if (elementindex == 4)
                {
                    GlobalProperties.schedule.Add(new TitanObj
                    {
                        element = $"Fire {hp}",
                        level = i + 1
                    });
                    elementindex = 0;
                }
            }
            FileHandler.SaveTitanFile();
        }

        /// <summary>
        /// checks the lmk list if the level is up it fires
        /// </summary>
        /// <returns></returns>
        public static async Task LmkChecker()
        {

            foreach (var i in GlobalProperties.lmkList)
            {
                Console.WriteLine($"lmk: {i.member.Username},{i.IsDm},{i.Complete}");
                if (i.Complete == false)
                {
                    if (i.level <= GlobalProperties.currentLevel)
                    {
                        if (i.IsDm)
                            await i.member.SendMessageAsync(SendBlue($"Hey, {i.member.Username}! The Titan has been set to level {GlobalProperties.currentLevel}."));
                        else
                            await Program.PostToCommandChannel(null, $"Hey, {i.member.Mention}! The Titan has been set to level {GlobalProperties.currentLevel}.");
                        i.Complete = true;
                    }
                }
            }

            //empty all the trues
            GlobalProperties.lmkList.RemoveAll(p => p.Complete == true);

        }

        #region Context Extension Methods
        /// <summary>
        /// Posted when a user who does not have permission to use a command
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task PermissionError(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Permissions Error",
                Description = "You do not have permission to use this command.",
                Color = new DiscordColor(0xFF0000) // red
            };

            await ctx.RespondAsync("", embed: embed);
        }


        /// <summary>
        /// Call this command to post !list in the read only channel
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task ListTitanReadonly(CommandContext ctx)
        {
            await ClearReadOnlyChannel();
            try
            {
                int count = TitanCountBasedOffCurrentLevel();
                string output = "";
                int countindex = 0;
                int index = 0;
                foreach (var s in GlobalProperties.schedule)
                {
                    if (index >= GlobalProperties.currentLevel - 1)
                    {
                        if (countindex < count)
                        {
                            string x = (s.reserved.Count > 0) ? string.Join(", ", s.reserved).UnSanitize() : "None";
                            output += $"Lvl {s.level} {s.element.UnSanitize()}    Rsvp:  {x} \r\n";
                            countindex++;
                        }
                        else
                        {
                            break;
                        }

                    }
                    index++;
                }
                await Program.PostToReadonlyChannel(ctx, $"_**The next {count} Titans**_ \r\n" + Utils.SendGreen(output));
            }
            catch (Exception e)
            {
                await ErrorHandler(ctx, e);
            }
        }
        /// <summary>
        /// method for getting the number of titans to show based on current level. ie: on titan level 1, show the next 30 titans
        /// </summary>
        /// <returns></returns>
        public static int TitanCountBasedOffCurrentLevel()
        {
            if (GlobalProperties.currentLevel < 10)
                return 30;
            if (GlobalProperties.currentLevel < 25)
                return 25;
            if (GlobalProperties.currentLevel < 35)
                return 15;
            if (GlobalProperties.currentLevel < 40)
                return 10;
            return 5;
        }
        public static async Task ClearReadOnlyChannel()
        {
            try
            {
                await Program.GetReadOnlyChannel();
                var allmsg = await GlobalProperties.ReadOnlyChannel.GetMessagesAsync();
                await GlobalProperties.ReadOnlyChannel.DeleteMessagesAsync(allmsg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        /// <summary>
        /// Handler to post errors to everywhere needed
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task ErrorHandler(CommandContext ctx = null, Exception e = null)
        {
            if (e != null)
                Console.WriteLine(e);
            if (ctx != null)
            {
                if (e != null)
                    await ctx.Message.RespondAsync(Utils.SendGreen($"Error of type {e.GetType().ToString()} please check the logs"));
                else
                    await ctx.Message.RespondAsync(Utils.SendGreen("An error occured, please check the logs"));
            }
            FileHandler.SaveToLog(e.ToString());
        }


        /// <summary>
        /// Check if user is an officer. false if not
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool IsOfficer(CommandContext ctx, bool setup)
        {
            if (!setup)
                return true;
            try
            {
                bool officer = false;
                foreach (var i in ctx.Member.Roles)
                {
                    if (GlobalProperties.OfficerList.Contains(i.Name))
                    {
                        officer = true;
                        break;
                    }

                }
                return officer;
            }
            catch
            {
                Console.WriteLine("Cant tell if officer because of DM.");
                return false;
            }
        }
        public static bool IsMuted(CommandContext ctx)
        {
            if (GlobalProperties.mutedList.Contains(ctx.Message.Author.Username.ToString()))
                return true;
            return false;
        }
        public static async Task SetStatus()
        {
            var battleTime = GetCurrentTimeFrame();
            if (battleTime.TimeFrame == TimeFrame.AfternoonBattle || battleTime.TimeFrame == TimeFrame.MorningBattle)
                await Program.SetStatus(UserStatus.Online);
            else
                await Program.SetStatus(UserStatus.Idle);

        }

        /// <summary>
        /// Deletes the command message when deleteCommands bool is true
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task DeleteMessage(CommandContext ctx)
        {
            if (GlobalProperties.deleteCommands)
                await ctx.Message.DeleteAsync();
        }
        /// <summary>
        /// returns if channel is a "quiet channel" where the bot does not talk back regardless of the command
        /// </summary>
        /// 
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool IsQuiet(CommandContext ctx)
        {
            if(GlobalProperties.QuietModeList.Contains(ctx.Message.Channel.Name))
            {
                RespondWithReaction(ctx);
                return true;
            }
            else
                return false;
        }
        public static bool IsQuietPermissionDenied(CommandContext ctx)
        {
            if (ctx.Message.Channel.Name == "general")
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":poop:");
                ctx.Message.CreateReactionAsync(emoji);
                return true;
            }
            else
                return false;
        }
        public static async Task RespondWithReaction(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
            await ctx.Message.CreateReactionAsync(emoji);
        }
        #endregion

       

        #region Text Formatting Methods
        public static string SendBold(string s)
        {
            return "**" + s + "**";
        }
        public static string SendItalic(string s)
        {
            return "_" + s + "_";
        }
        public static string SendUnderline(string s)
        {
            return "__" + s + "__";
        }
        public static string SendUnderlineItalic(string s)
        {
            return "__*" + s + "*__";
        }
        /// <summary>
        /// uses dsconfig to write in blue text
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SendBlue(string s)
        {
            //dsconfig
            return @"```
" + s + @"
```";
        }
        /// <summary>
        /// uses CSS tags to write in green text
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SendGreen(string s)
        {
            //css
            return @"```
" + s + @"
```";
        }
        #endregion
    }

    /// <summary>
    /// object for holding current time frame, time till next battle etc.
    /// </summary>
    public class BattleTime
    {
        public string Time;
        public TimeFrame TimeFrame;
    }
    public class TitanObj
    {
        public string element;
        public int level;
        public List<string> reserved = new List<string>();
    }
    public class LmkObject
    {
        public DiscordMember member;
        public int level;
        public bool IsDm = true;
        public bool Complete = false;
    }
    public static class StringExtensions
    {
        /// <summary>
        /// replace punctuation from string with [type] for quick unsantization
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String Sanitize(this String s)
        {
            return s.Replace(",", "[COMMA]")
                .Replace(".", "[PERIOD]")
                .Replace("\'", "[APOSTROPHE]")
                .Replace("\"", "[QUOTE]")
                .Replace("{", "[LBRACE]")
                .Replace("}", "[RBRACE]")
                .Replace(";", "[SEMICOLON]")
                .Replace(":", "[COLON]")
                .Replace("/", "[FSLASH]")
                .Replace("\\", "[BSLASH]");
        }
        /// <summary>
        /// unsanitize strings who have had punctuation replaced for file saving
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String UnSanitize(this String s)
        {
            return s.Replace("[PERIOD]", ".")
                .Replace("[COMMA]", ",")
                .Replace("[APOSTROPHE]", "\'")
                .Replace("[QUOTE]", "\"")
                .Replace("[LBRACE]", "{")
                .Replace("[RBRACE]", "}")
                .Replace("[SEMICOLON]", ";")
                .Replace("[COLON]", ":")
                .Replace("[FSLASH]", "/")
                .Replace("[BSLASH]", "\\");
        }

        /// <summary>
        /// remove all punctuation from a string for interactivity demo
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String RemovePunc(this String s)
        {
            return s.Replace(".", "")
                    .Replace(",", "")
                    .Replace("\"", "")
                    .Replace("\'", "")
                    .Replace("!", "")
                    .Replace("\\", "")
                    .Replace("/", "");
        }
        /// <summary>
        /// simplify the input string to replace "you" with "u", "are" with "r" etc. for efficient ai parsing
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String Simplify(this String s)
        {
            return s.Replace("you", "u")
                    .Replace("are", "r")
                    .Replace("hello", "hi")
                    .Replace(" i ", " ")
                    .Replace("titan-bot", "%")
                    .Replace("titan bot", "%")
                    .Replace("your","ur")
                    .Replace("youre","ur");
        }
    }

    internal static class DictionaryHelper
    {
        /// <summary>
        /// Returns a string of key value pairs in the format k1=v1,k2=v2,...
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return ToKeyValueString(dictionary, "{0}={1}", ",");
        }

        /// <summary>
        /// Returns a string of key value paris in the format k1=v1{separator}kk2=v2...
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="seperator">The string separator for the pairs</param>
        /// <returns></returns>
        public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string seperator)
        {
            return ToKeyValueString(dictionary, "{0}={1}", seperator);
        }

        /// <summary>
        /// Returns a string of key value pairs in the specified format with the specified separator
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="format">The format string for the key value pairs</param>
        /// <param name="separator">The string separator for the pairs</param>
        /// <returns></returns>
        public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string format, string separator)
        {
            var pairs = dictionary.Select(c => string.Format(format, c.Key, c.Value));
            return string.Join(separator, pairs);
        }
    }
}