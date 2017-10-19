using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titan_Bot
{
    [RequireOwner]
    public class OwnerCommands
    {
        [Command("add-officer"), Description("Adds an officer to your officer only commands. Must enter whole role name")]
        public async Task AddOfficer(CommandContext ctx, string name)
        {
            if (Utils.IsOfficer(ctx, GlobalProperties.IsSetup))
            {
                GlobalProperties.OfficerList.Add(name);
                FileHandler.SaveSettings();
                await ctx.RespondAsync($"The Role `{name}` has been added to the permissions list");
            }
        }
        [Command("install"), Description("Installs the requisite channels and settings to the server")]
        public async Task InstallBot(CommandContext ctx)
        {
            if (GlobalProperties.OfficerList.Count < 1)
            {
                await ctx.RespondAsync("Please set at least 1 role with !add-officer before calling install");
                return;
            }
            if (!GlobalProperties.IsSetup)
            {
                //create category
                var cat = await ctx.Guild.CreateChannelAsync("Titan", ChannelType.Category);
                var list = await ctx.Guild.CreateChannelAsync("titan-current-list", ChannelType.Text, cat);
                var commands = await ctx.Guild.CreateChannelAsync("titan-commands", ChannelType.Text, cat);
                GlobalProperties.IsSetup = true;
                GlobalProperties.ReadOnlyId = list.Id;
                GlobalProperties.CommandChannelId = commands.Id;
                FileHandler.SaveSettings();
                await ctx.RespondAsync("Channels have been created. Make sure to set the permissions for #titan-current-list to read-only for all roles but the bot!");
                await ctx.RespondAsync("Make sure to use the command !general-chat so the bot knows which channel to announce battle has started!");
                await ctx.RespondAsync("Tip: Set your general chat channels in !quiet-channel so the bot doesn't clutter up chat with command responses!");
                await ctx.RespondAsync("Any changes can be made by viewing the `settings.txt` file.");
            }
            else
            {
                await ctx.RespondAsync("Server has already been set up. Please delete the `settings.txt` file to reset the server");
            }
        }
        [Command("quiet-channel"), Description("Sets the channel to quiet mode, where the bot will not use text to respond to most commands. Give channel name as parameter without #")]
        public async Task QuietChannel(CommandContext ctx, string chid)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            //set to lowercase
            GlobalProperties.QuietModeList.Add(chid.ToLower());
            FileHandler.SaveSettings();
            await ctx.RespondAsync($"Channel `{chid}` has been set to quiet mode. Most commands will not be responded with a text message.");
        }
        [Command("general-chat"), Description("Sets the general channel. Set this with the channel id if you'd like the bot to announce when battle starts")]
        public async Task GeneralChannel(CommandContext ctx, string chid)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            GlobalProperties.GeneralChannelId = ulong.Parse(chid);
            FileHandler.SaveSettings();
            await ctx.RespondAsync($"Channel with the id `{chid}` has been set as the server general chat.");
        }
        [Command("set-times"), Description("Set battle times in military format.")]
        public async Task SetTimes(CommandContext ctx, [Description("Afternoon Start")]string afts, [Description("Afternoon End")]string afte,
            [Description("Evening Start")]string eves, [Description("Evening End")]string evee)
        {
            GlobalProperties.AfternoonStart = afts;
            GlobalProperties.AfternoonEnd = afte;
            GlobalProperties.EveningStart = eves;
            GlobalProperties.EveningEnd = evee;
            FileHandler.SaveSettings();
            await ctx.RespondAsync("Battle times have been changed");
        }
        [Command("sethp"), Description("Sets the list hp for the titan. Accepts format such as xx.xx such as 12.5, 34.05 etc")]
        public async Task SetHp(CommandContext ctx, int lvl, string hp)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            try
            {
                await Utils.DeleteMessage(ctx);
                
                    Console.WriteLine(lvl + " " + hp);
                    GlobalProperties.titanHpList[lvl] = hp.Sanitize();
                    FileHandler.SaveTitanHpList();
                    Utils.IsQuiet(ctx);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
