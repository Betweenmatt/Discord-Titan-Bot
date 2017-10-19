using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titan_Bot
{
    public static class FileHandler
    {
        private static string appdir = AppDomain.CurrentDomain.BaseDirectory;
        public static void SaveTitanFile()
        {
            try
            {
                string output = "";
                foreach (var i in GlobalProperties.schedule)
                {
                    output += i.level + "." + i.element + "." + string.Join(",", i.reserved) + ";";
                }
                using (System.IO.FileStream fs = System.IO.File.Create(appdir + "titanfile.txt"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(output);
                    fs.Write(info, 0, info.Length);

                    // writing data in bytes already
                    byte[] data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void SaveTitanHpList()
        {
            try
            {
                /*
                string output = "";

                foreach (var i in GlobalProperties.titanHpList)
                {
                    output += i.Key + "," + i.Value + ";\r\n";
                }
                */
                string output = GlobalProperties.titanHpList.ToKeyValueString("{0},{1}", ";");
                //output = output.TrimEnd(';');//remove last seperator char. idt this is working tho
                using (System.IO.FileStream fs = System.IO.File.Create(appdir + "titanhplist.txt"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(output);
                    fs.Write(info, 0, info.Length);

                    // writing data in bytes already
                    byte[] data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void OpenTitanFile()
        {
            try
            {
                GlobalProperties.schedule = new List<TitanObj>();
                string[] infofile = System.IO.File.ReadAllText(appdir + "titanfile.txt").Split(';');
                int outofboundsindex = 0;
                foreach (var i in infofile)
                {
                    if (outofboundsindex < infofile.Length -1)
                    {
                        string[] splode = i.Split('.');
                        List<string> tempreserve = new List<string>();
                        if (splode.Length > 2)
                        {
                            if (splode[2].Contains(","))
                                tempreserve = splode[2].Split(',').ToList();
                            else if(splode[2] != "")
                                tempreserve.Add(splode[2]);
                        }
                        GlobalProperties.schedule.Add(new TitanObj()
                        {
                            level = int.Parse(splode[0]),
                            element = splode[1],
                            reserved = tempreserve
                        });
                        outofboundsindex++;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Utils.ResetTitanSchedule();
            }
        }
        public static void OpenTitanHpList()
        {
            try
            {
                GlobalProperties.titanHpList = System.IO.File.ReadAllText(appdir + "titanhplist.txt").Split(';')
                    .ToDictionary(x => int.Parse(x.Split(',')[0]), x => x.Split(',')[1]) ?? new Dictionary<int, string>();
            }
            catch
            {
                Console.WriteLine("No HP file found, skipping titan hp condition");
            }
        }
        public static void OpenSettings()
        {
            try
            {
                string infofile = System.IO.File.ReadAllText(@appdir + "settings.txt");
                GlobalProperties.IsSetup = (infofile.Split(new string[] { "<IsSetup>" }, StringSplitOptions.None)[1] == "True") ? true : false;
                GlobalProperties.currentLevel = int.Parse(infofile.Split(new string[] { "<currentLevel>" }, StringSplitOptions.None)[1]);
                GlobalProperties.deleteCommands = (infofile.Split(new string[] { "<deleteCommands>" }, StringSplitOptions.None)[1] == "True") ? true : false;
                GlobalProperties.mutedList = infofile.Split(new string[] { "<mute>" }, StringSplitOptions.None)[1].Split(',').ToList() ?? new List<string>();
                GlobalProperties.CommandChannelId = ulong.Parse(infofile.Split(new string[] { "<commandChannelId>" }, StringSplitOptions.None)[1]);
                GlobalProperties.ReadOnlyId = ulong.Parse(infofile.Split(new string[] { "<readOnlyChannelId>" }, StringSplitOptions.None)[1]);
                GlobalProperties.QuietModeList = infofile.Split(new string[] { "<quiet>" }, StringSplitOptions.None)[1].Split(',').ToList() ?? new List<string>();
                GlobalProperties.Token = infofile.Split(new string[] { "<Token>" }, StringSplitOptions.None)[1];
                GlobalProperties.GeneralChannelId = ulong.Parse(infofile.Split(new string[] { "<generalChannelId>" }, StringSplitOptions.None)[1]);
                GlobalProperties.AfternoonStart = infofile.Split(new string[] { "<afternoonStart>" }, StringSplitOptions.None)[1];
                GlobalProperties.AfternoonEnd = infofile.Split(new string[] { "<afternoonEnd>" }, StringSplitOptions.None)[1];
                GlobalProperties.EveningStart = infofile.Split(new string[] { "<eveningStart>" }, StringSplitOptions.None)[1];
                GlobalProperties.EveningEnd = infofile.Split(new string[] { "<eveningEnd>" }, StringSplitOptions.None)[1];
                GlobalProperties.OfficerList = infofile.Split(new string[] { "<officerList>" }, StringSplitOptions.None)[1].Split(',').ToList() ?? new List<string>();
            }
            catch
            {
                //if file doesn't exist create it
                SaveSettings();
            }
        }
        public static void SaveSettings()
        {
            string output = $"<currentLevel>{GlobalProperties.currentLevel}<currentLevel>" +
                $"<deleteCommands>{GlobalProperties.deleteCommands}<deleteCommands>" +
                $"<mute>{string.Join(",",GlobalProperties.mutedList)}<mute>" +
                $"<IsSetup>{GlobalProperties.IsSetup}<IsSetup>" +
                $"<commandChannelId>{GlobalProperties.CommandChannelId}<commandChannelId>" +
                $"<readOnlyChannelId>{GlobalProperties.ReadOnlyId}<readOnlyChannelId>" +
                $"<Token>{GlobalProperties.Token}<Token>" +
                $"<quiet>{string.Join(",",GlobalProperties.QuietModeList)}<quiet>" +
                $"<generalChannelId>{GlobalProperties.GeneralChannelId}<generalChannelId>" +
                $"<afternoonStart>{GlobalProperties.AfternoonStart}<afternoonStart>" +
                $"<afternoonEnd>{GlobalProperties.AfternoonEnd}<afternoonEnd>" +
                $"<eveningStart>{GlobalProperties.EveningStart}<eveningStart>" +
                $"<eveningEnd>{GlobalProperties.EveningEnd}<eveningEnd>" +
                $"<officerList>{string.Join(",",GlobalProperties.OfficerList)}<officerList>";
            using (System.IO.FileStream fs = System.IO.File.Create(appdir + "settings.txt"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(output);
                fs.Write(info, 0, info.Length);

                // writing data in bytes already
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }
        }
        /// <summary>
        /// saves to log file named by this date
        /// </summary>
        /// <param name="s"></param>
        public static void SaveToLog(string s)
        {
            try
            {
                System.IO.Directory.CreateDirectory(appdir + "Logs/");
                String timeStamp = GetTimestamp(DateTime.Now);
                string thisdate = DateTime.Now.ToString("MM-dd-yyyy");
                string previous = "";
                try
                {
                    previous = System.IO.File.ReadAllText(appdir + "Logs/" + $"Log_{thisdate}.txt");
                }
                catch
                {
                    previous = "";
                }
                string output = previous + $"{timeStamp}: {s}\r\n";

                using (System.IO.FileStream fs = System.IO.File.Create(appdir + "Logs/" + $"Log_{thisdate}.txt"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(output);
                    fs.Write(info, 0, info.Length);

                    // writing data in bytes already
                    byte[] data = new byte[] { 0x0 };
                    fs.Write(data, 0, data.Length);
                }
            }catch(Exception e)
            {
                Console.WriteLine("Error with logging \r\n" + e);
            }
        }
        /// <summary>
        /// convert datetime to hh:mm:ss
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static String GetTimestamp(DateTime value)
        {
            return value.ToString("hh:mm:ss");
        }
    }
}
