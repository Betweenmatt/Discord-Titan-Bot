using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Titan_Bot;

class Program
{

    static Interactivity ai = new Interactivity();

    static ConsoleEventDelegate handler;

    static void Main(string[] args)
    {
        handler = new ConsoleEventDelegate(ConsoleEventCallback);
        SetConsoleCtrlHandler(handler, true);

        FileHandler.OpenTitanHpList();
        FileHandler.OpenTitanFile();//reset schedule on load or pull schedule file
        FileHandler.OpenSettings();//get settings
        if(GlobalProperties.Token == "")
        {
            Console.WriteLine("Enter in your bot's token.");
            string getToken = Console.ReadLine();
            GlobalProperties.Token = getToken;
            FileHandler.SaveSettings();
        }
        TimeLapse.StartTimer();
        MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    static bool ConsoleEventCallback(int eventType)
    {
        if (eventType == 2)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2HM9H7XUUUAM4");
        }
        return false;
    }
    static async Task MainAsync(string[] args)
    {
        GlobalProperties.discord = new DiscordClient(new DiscordConfiguration
        {//MzU5MTk5NjAwMjE0NTQwMjg5.DKDjSA.E9in_YB9nQ3ZhX0Q4_O8z3_KL2g
            Token = GlobalProperties.Token,
            TokenType = TokenType.Bot,
            UseInternalLogHandler = true,
            LogLevel = LogLevel.Debug

        });
        GlobalProperties.discord.SetWebSocketClient<WebSocket4NetCoreClient>();

        GlobalProperties.commands = GlobalProperties.discord.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefix = "!",
            EnableDefaultHelp = true,
            CaseSensitive = false
        });
        GlobalProperties.commands.RegisterCommands<Commands>();
        GlobalProperties.commands.RegisterCommands<OfficerCommands>();
        GlobalProperties.commands.RegisterCommands<OwnerCommands>();
        GlobalProperties.discord.MessageCreated += async (e) =>
        {
            if (!e.Message.Author.IsBot)
            {
                if (e.Message.Content.ToLower().Contains("titan-bot") || e.Message.Content.ToLower().Contains("titan bot"))
                {
                    if (e.Message.Content.ToLower().RemovePunc().Simplify().Contains("can u check ur dictionary"))
                    {
                        if (e.Message.Author.Username == "Tastygod")
                        {
                            await ai.Respond(e, "Of course Tastygod, I'll get right on that!");
                            ai.GetContextJson();
                        }
                        else
                        {
                            await ai.Respond(e, "Sorry, @. This function is only available to Tastygod!");
                        }
                    }
                    else if (e.Message.Content.ToLower() == "bad titan-bot" || e.Message.Content.ToLower() == "bad titan bot")
                    {
                        await ai.Respond(e,":(");
                    }
                    else if (e.Message.Content.ToLower() == "good titan-bot" || e.Message.Content.ToLower() == "good titan bot")
                    {
                        await ai.Respond(e, ":)");
                    }
                    else
                    {
                        await ai.Interact(e);
                    }
                }
            }
        };
        //on ready
        GlobalProperties.discord.Ready += async e =>
        {
            await Task.Yield(); //
            GlobalProperties.discord.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Ready! Setting status message..", DateTime.Now);
            await Utils.SetStatus();
            //await OpeningMessage();
        };

        await GlobalProperties.discord.ConnectAsync();
        await Task.Delay(-1);
    }
    /// <summary>
    /// Welcome message when bot is loaded
    /// </summary>
    /// <returns></returns>
    private static async Task OpeningMessage()
    {
#if !DEBUG
        await PostToCommandChannel(null, "Beep Boop Titan-Bot Online!");
        await PostToReadonlyChannel(null, "Beep Boop Titan-Bot Online!");
#endif
    }

    private Task Log(string msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public static async Task PostToReadonlyChannel(CommandContext ctx, string msg)
    {
        await GetReadOnlyChannel();
        await GlobalProperties.discord.SendMessageAsync(GlobalProperties.ReadOnlyChannel, msg);//post message
    }
    public static async Task PostToCommandChannel(CommandContext ctx, string msg)
    {
        if(GlobalProperties.CommandChannel == null)
            GlobalProperties.CommandChannel = await GlobalProperties.discord.GetChannelAsync(GlobalProperties.CommandChannelId);
        await GlobalProperties.discord.SendMessageAsync(GlobalProperties.CommandChannel, msg);
    }

    public static async Task GetReadOnlyChannel()
    {
        if (GlobalProperties.ReadOnlyChannel == null)
            GlobalProperties.ReadOnlyChannel = await GlobalProperties.discord.GetChannelAsync(GlobalProperties.ReadOnlyId);//get the readonly channel
        else
            return;
    }
    public static async Task SetReadOnlyTopic(CommandContext ctx, string topic)
    {
        if (GlobalProperties.ReadOnlyChannel == null)
            GlobalProperties.ReadOnlyChannel = await GlobalProperties.discord.GetChannelAsync(GlobalProperties.ReadOnlyId);//get the readonly channel
        await GlobalProperties.ReadOnlyChannel.ModifyAsync(null, null, topic);
    }
    public static async Task PostToGeneralChannel(CommandContext ctx, string msg)
    {
        if (GlobalProperties.GeneralChannel == null)
            GlobalProperties.GeneralChannel = await GlobalProperties.discord.GetChannelAsync(GlobalProperties.GeneralChannelId);
        await GlobalProperties.discord.SendMessageAsync(GlobalProperties.GeneralChannel, msg);
    }
    
    /// <summary>
    /// Set the bots status, idle online dnd etc.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static async Task SetStatus(UserStatus s)
    {
        await GlobalProperties.discord.UpdateStatusAsync((s == UserStatus.Online) ? GlobalProperties.thisGame : GlobalProperties.waitingGame, s);
    }

    private delegate bool ConsoleEventDelegate(int eventType);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
}