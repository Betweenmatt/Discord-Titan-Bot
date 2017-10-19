using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Titan_Bot
{
    public class Interactivity
    {
        private static string appdir = AppDomain.CurrentDomain.BaseDirectory;
        List<AIObject> basicList = new List<AIObject>();
        public Interactivity()
        {
            GetContextJson();
        }
        public void GetContextJson()
        {
            try
            {
                var contextFile = System.IO.File.ReadAllText(appdir + "aiContext.txt");
                var result = JsonConvert.DeserializeObject<List<AIObject>>(contextFile);
                basicList = result;
            }
            catch(Exception e)
            {
                Console.WriteLine("No interactivity file found, skipping interactivity section");
                Console.WriteLine(e);
            }
        }

        public async Task Interact(MessageCreateEventArgs e)
        {
            string content = e.Message.Content.ToLower().RemovePunc().Simplify();
            foreach(var i in basicList)
            {
                if (CatcherCheck(i, content))
                {
                    if(Context(content, i.context))
                    {
                        string response = GetRandomResponse(i.response);
                        await Respond(e, response);
                        return;
                    }
                }
            }
            //if no context is appropriate, at least reaspond with a emoji response for using bots name
            
        }
        private bool CatcherCheck(AIObject ai, string content)
        {
            foreach(var i in ai.catcher) {
                if (content.Contains(i))
                    return true;
                
            }
            return false;
        }
        /// <summary>
        /// checks array for string, if context is appropriate, returns true
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arr"></param>
        /// <returns></returns>
        private bool Context(string s, string[] arr)
        {
            foreach(var i in arr)
            {
                if (s.Contains(i))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// gets a random response from a response array
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static string GetRandomResponse(string[] arr)
        {
            Random rnd = new Random();
            int index = rnd.Next(0, arr.Length - 1);
            return arr[index];
        }
        /// <summary>
        /// respond with this method to replace @ with mention
        /// </summary>
        /// <param name="e"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public async Task Respond(MessageCreateEventArgs e, string s)
        {

            await e.Channel.TriggerTypingAsync();
            Sleep(2);
            string output = s.Replace("@", e.Message.Author.Mention);
            await e.Message.RespondAsync(output);
        }
        public async Task RespondWithEmoji(MessageCreateEventArgs e, DiscordEmoji emoji = null)
        {
            if(emoji == null)
                emoji = DiscordEmoji.FromName(GlobalProperties.discord, ":heart:");
            Sleep(1);
            await e.Message.CreateReactionAsync(emoji);
        }
        /// <summary>
        /// sleeps thread
        /// </summary>
        /// <param name="seconds"></param>
        private static void Sleep(int seconds)
        {
            int toMS = seconds * 1000;
            Thread.Sleep(toMS);
        }
    }
    class AIObject
    {
        /// <summary>
        /// optional name
        /// </summary>
        public string name;
        /// <summary>
        /// list of terms or words that indicate this object may have an appropriate context
        /// </summary>
        public string[] catcher;
        /// <summary>
        /// list of appropriate contexts to respond to
        /// </summary>
        public string[] context;
        /// <summary>
        /// response list able to grab by RNG index
        /// </summary>
        public string[] response;
    }
}
