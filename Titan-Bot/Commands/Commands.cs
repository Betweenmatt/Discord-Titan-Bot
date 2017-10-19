using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;

namespace Titan_Bot
{
    public class Commands
    {
       #region USELESS COMMANDS
        /// <summary>
        /// useless random number generator
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [Command("random"), Description("Gives a random number")]
        public async Task GetRandomNumber(CommandContext ctx, int min = 0, int max = 100)
        {
            await Utils.DeleteMessage(ctx);
            var rnd = new Random();

            await ctx.RespondAsync($"🎲 Your random number is: { rnd.Next(min, max)}");
        }
        #endregion



        /// <summary>
        /// Toggle for auto-delete function
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        
        /// <summary>
        /// print the list of titan elements in order
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("elements"), Description("List the titan elements in order")]
        public async Task GetElementList(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            string output = "Dark" + "\r\n" + "Water" + "\r\n" + "Wood" + "\r\n" + "Light" + "\r\n" + "Fire";
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Titan Element List",
                Description = output,
                Color = new DiscordColor(0x66ffff) // turquoise
            };

            if (!Utils.IsQuiet(ctx))
                await ctx.RespondAsync("", embed: embed);
        }
        /// <summary>
        /// list the titans and their reservations, default = 5, max = 40
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [Command("list"), Description("Displays a list of the titans and each reservation. Defaults to the next 5 titans.")]
        public async Task GetCurrentRSVPList(CommandContext ctx, [Description("Number of titans to show, default 5, max 40")] int count = 5, int sl = 0)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            if (count > 40)
                count = 40;
            await Utils.DeleteMessage(ctx);
            try
            {
                string output = "";
                int countindex = 0;
                int index = 0;
                int localLevel = (sl == 0) ? GlobalProperties.currentLevel : sl;
                foreach (var s in GlobalProperties.schedule)
                {

                    if (index >= localLevel - 1)
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
                if (!Utils.IsQuiet(ctx))
                    await ctx.RespondAsync($"_**The next {count} Titans**_ \r\n" + Utils.SendGreen(output));
            }
            catch (Exception e)
            {
                await Utils.ErrorHandler(ctx, e);
            }
            await Utils.SetStatus();
        }
        [Command("lmk"), Description("Let Me Know - When titan level is set to the level you specify, the bot will DM you a reminder.")]
        public async Task LMKCommand(CommandContext ctx, [Description("The level you want to be notified of")]int level,
            [Description("Set this to false if you want @mention instead of DM. Defaults to true")]bool isDm = true)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            if (ctx.Member != null)
            {
                GlobalProperties.lmkList.Add(new LmkObject
                {
                    member = ctx.Member,
                    level = level,
                    IsDm = isDm,
                    Complete = false
                });
                if (!Utils.IsQuiet(ctx))
                    await ctx.Message.RespondAsync(Utils.SendBlue($"Titan-Bot will let you know when Titan level {level} is up!"));
                
            }
            else
            {
                await ctx.Message.RespondAsync(Utils.SendGreen("You can not set !lmk with a DM :( If you're not in a DM then something went wrong!"));
            }
        }
        
        /// <summary>
        /// gets the time until next titan, or time left in current titan if one is already active
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("next"), Description("Gets the time until the next titan. Returns time left if titan is currently active.")]
        public async Task GetNextTitan(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            var battleTime = Utils.GetCurrentTimeFrame();
            if(battleTime.TimeFrame == TimeFrame.MorningBattle)
                await ctx.Message.RespondAsync(Utils.SendBlue($"Battle current active, Time remaining is {battleTime.Time}"));
            if(battleTime.TimeFrame == TimeFrame.AfternoonBattle)
                await ctx.Message.RespondAsync(Utils.SendBlue($"Battle current active, Time remaining is {battleTime.Time}"));
            if (battleTime.TimeFrame == TimeFrame.MorningWait)
                await ctx.Message.RespondAsync(Utils.SendBlue($"Time remaining until next battle is {battleTime.Time}"));
            if (battleTime.TimeFrame == TimeFrame.AfternoonWait)
                await ctx.Message.RespondAsync(Utils.SendBlue($"Time remaining until next battle is {battleTime.Time}"));
            await Utils.SetStatus();

        }
        
        /// <summary>
        /// user reserve spot for specific titan
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        [Command("rsvp"), Description("Reserves the player for a specific titan")]
        [Aliases("r")]
        public async Task RsvpTitan(CommandContext ctx, [Description("Titan Level to reserve")] int level, 
            [Description("Additional notes such as expected dmg or expecting to kill")] string desc = "", 
            [Description("Player name to reserve, leave blank to use UserName")] string name = "")
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            if (name == "")
                name = ctx.User.Username;
            name = name.Sanitize();
            desc = desc.Sanitize();
            await Utils.DeleteMessage(ctx);
            string input = name;
            if (desc != "")
                input += $"({desc})";
            GlobalProperties.schedule[level - 1].reserved.Add(input);
            if(!Utils.IsQuiet(ctx))
                await ctx.Message.RespondAsync(ctx.User.Mention+Utils.SendBlue($" Thanks for RSVPing level {level}!"));
            await Utils.ListTitanReadonly(ctx);
            FileHandler.SaveTitanFile();
            await Utils.SetStatus();
        }
        /// <summary>
        /// remove player from rsvp list at specific boss. can use fraction of a name like !rsvp-remove 50 Tasty to remove @Tastygod,
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Command("rsvp-remove"), Description("Remove a player from the reserved list")]
        public async Task RemoveRsvp(CommandContext ctx, [Description("Titan Level")] int level, [Description("Name of the player to be removed. Can use partial name")] string name)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            var temp = GlobalProperties.schedule[level - 1].reserved;
            foreach (var x in GlobalProperties.schedule[level - 1].reserved)
            {
                if (x.Contains(name))
                    temp.Remove(x);
            }
            GlobalProperties.schedule[level - 1].reserved.Remove(name);

            if (!Utils.IsQuiet(ctx))
                await ctx.Message.RespondAsync(Utils.SendGreen($"Removed rsvp for level {level}"));
            await Utils.ListTitanReadonly(ctx);
            FileHandler.SaveTitanFile();
            await Utils.SetStatus();
        }
        /// <summary>
        /// set current titan level to help with list command
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [Command("setlevel"), Description("Sets the current titan level")]
        [Aliases("setlvl")]
        public async Task SetCurrentLevel(CommandContext ctx, int level)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            GlobalProperties.currentLevel = level;
            FileHandler.SaveSettings();
            if (!Utils.IsQuiet(ctx))
                await ctx.Message.RespondAsync(Utils.SendBlue($"The Titan level is set to {level}"));
            await Utils.ListTitanReadonly(ctx);
            await Utils.LmkChecker();
            await Utils.SetStatus();
        }
        
        [Command("currenthp"), Description("Sets the hp provided as the general topic. Used for current titan hp")]
        public async Task CurrentHp(CommandContext ctx, int lvl, string hp)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);

            GlobalProperties.currentTitanHp = hp;
                await Program.SetReadOnlyTopic(ctx, $"Lvl: {lvl} {QuickGetElement(lvl)} : {hp}");
                await Utils.RespondWithReaction(ctx);
        }
        private string QuickGetElement(int l)
        {
            int colorindex = 0;
            for(int i = 0; i < 100; i++)
            {
                if (i == l - 1)
                    break;
                if (colorindex == 4)
                    colorindex = 0;
                else
                    colorindex++;
            }
            string color = "";
            if (colorindex == 0)
                color = "Dark";
            if (colorindex == 1)
                color = "Water";
            if (colorindex == 2)
                color = "Wood";
            if (colorindex == 3)
                color = "Light";
            if (colorindex == 4)
                color = "Fire";
            return color;
        }
        
        

        //dont touch
        [Command("test"), RequireRolesAttribute("")]
        public async Task TestMethod(CommandContext ctx, string name, string msg)
        {
            
        }

    }

}
