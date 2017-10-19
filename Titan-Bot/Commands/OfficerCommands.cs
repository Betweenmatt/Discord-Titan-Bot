using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titan_Bot
{
    public class OfficerCommands
    {
        [Command("auto-delete"), Description("*OFFICER ONLY COMMAND* Toggles the command to auto delete command messages")]
        public async Task AutoDeleteToggle(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                await Utils.DeleteMessage(ctx);
                GlobalProperties.deleteCommands = (!GlobalProperties.deleteCommands) ? true : false;
                FileHandler.SaveSettings();
                if (!Utils.IsQuiet(ctx))
                    await ctx.Message.RespondAsync(Utils.SendGreen($"Auto-Delete has been set to {GlobalProperties.deleteCommands.ToString()}"));
                FileHandler.SaveToLog($"User {ctx.User.ToString()} adjusted auto delete");
            }
            else
            {
                if (!Utils.IsQuietPermissionDenied(ctx))
                    await Utils.PermissionError(ctx);
                FileHandler.SaveToLog($"User {ctx.User.ToString()} tried to adjust auto-delete");
            }
        }
        /// <summary>
        /// Cleans the channel of all messages made by bot
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("clean"), Description("*OFFICER ONLY COMMAND* Deletes all messages in the channel made by Titan-Bot")]
        public async Task CleanChat(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                await ctx.Message.DeleteAsync();
                await ctx.Message.RespondAsync(Utils.SendBold(Utils.SendGreen("Clearing all bot messages beep boop")));
                //await ctx.Channel.TriggerTypingAsync();
                try
                {
                    var allmsg = await ctx.Message.Channel.GetMessagesAsync();
                    List<DiscordMessage> botMsgList = new List<DiscordMessage>();
                    foreach (var i in allmsg)
                    {
                        if (i.Author.IsBot)
                        {
                            botMsgList.Add(i);
                        }
                    }
                    await ctx.Message.Channel.DeleteMessagesAsync(botMsgList);
                    FileHandler.SaveToLog($"User {ctx.User.ToString()} cleaned bot messages");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                await Utils.PermissionError(ctx);
                FileHandler.SaveToLog($"User {ctx.User.ToString()} tried to clean bot messages");
            }
        }
        [Command("mute"), Description("*OFFICER ONLY COMMAND* Mutes the given user from using commands")]
        public async Task MuteUser(CommandContext ctx, string name)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                GlobalProperties.mutedList.Add(name);
                FileHandler.SaveSettings();
                await ctx.Message.RespondAsync(Utils.SendBlue($"The user {name} has been muted."));
            }
            else
            {
                if (!Utils.IsQuietPermissionDenied(ctx))
                    await Utils.PermissionError(ctx);
            }
        }
        [Command("mute-list"), Description("*OFFICER ONLY COMMAND* Lists all muted players")]
        public async Task MuteList(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                try
                {
                    string output = "";
                    foreach (var i in GlobalProperties.mutedList)
                    {
                        output += i + "\r\n";
                    }
                    await ctx.Message.RespondAsync(Utils.SendBlue("Muted players: \r\n " + output));
                }
                catch
                {
                    await ctx.Message.RespondAsync(Utils.SendBlue("There are no muted players."));
                }
            }
            else
            {
                if (!Utils.IsQuietPermissionDenied(ctx))
                    await Utils.PermissionError(ctx);
            }
        }
        /// <summary>
        /// Reset titan schedule, use only on sunday or risk wiping all reservations
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("reset-bot"), Description("*OFFICER ONLY COMMAND* Reset the titan schedule and current level. Only do this on Sunday.")]
        public async Task ResetBot(CommandContext ctx)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                await Utils.DeleteMessage(ctx);
                Utils.ResetTitanSchedule();
                GlobalProperties.currentLevel = 0;
                FileHandler.SaveSettings();
                if (!Utils.IsQuiet(ctx))
                    await ctx.Message.RespondAsync(Utils.SendBlue("Titan Schedule Reset!"));
                FileHandler.SaveToLog($"User {ctx.User.ToString()} reset bot");
                await Program.PostToReadonlyChannel(ctx, @"` Titan Schedule Reset! `");
            }
            else
            {
                if (!Utils.IsQuietPermissionDenied(ctx))
                    await Utils.PermissionError(ctx);
                FileHandler.SaveToLog($"User {ctx.User.ToString()} tried to reset bot");
            }
        }
        [Command("unmute"), Description("*OFFICER ONLY COMMAND* Unmutes the given user from using commands")]
        public async Task UnmuteUser(CommandContext ctx, string name)
        {
            if (!GlobalProperties.IsSetup)
            {
                await ctx.RespondAsync("Please set up the bot first by calling !install");
                return;
            }
            if (Utils.IsMuted(ctx))
                return;
            await Utils.DeleteMessage(ctx);
            bool officer = Utils.IsOfficer(ctx, GlobalProperties.IsSetup);
            if (officer)
            {
                GlobalProperties.mutedList.Remove(name);
                await ctx.Message.RespondAsync(Utils.SendBlue($"The user {name} has been unmuted."));
                FileHandler.SaveSettings();
            }
            else
            {
                if (!Utils.IsQuietPermissionDenied(ctx))
                    await Utils.PermissionError(ctx);
            }
        }
    }
}
