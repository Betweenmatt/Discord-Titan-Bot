using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Titan_Bot
{
    public class TimeLapse
    {
        static bool FirstLoop = true;
        public static void StartTimer()
        {
            FirstTimeLoop();
        }

        static void FirstTimeLoop()
        {
            double PingInterval;
            var now = DateTime.Now;
            var timeToNextHour = now.Date.AddHours(now.Hour + 1) - now;
            PingInterval = MillisecondsToMinutes(timeToNextHour.TotalMinutes);
#if DEBUG
            Console.WriteLine(PingInterval / 60000);
#endif


            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Interval = PingInterval;
            aTimer.Enabled = true;
            aTimer.Elapsed += delegate
            {
                AfterTimeInterval();
                PingInterval = MillisecondsToMinutes(60);
                aTimer.Interval = PingInterval;
            };
        }
        static void AfterTimeInterval()
        {
            Console.WriteLine(DateTime.Now);
            var x = Utils.GetCurrentTimeFrame();
            if (x.TimeFrame == TimeFrame.AfternoonBattle || x.TimeFrame == TimeFrame.MorningBattle)
            {
                Program.SetStatus(DSharpPlus.Entities.UserStatus.Online);
                var now = DateTime.Now;
                if(now.DayOfWeek != DayOfWeek.Sunday)
                    if(now.Hour == 21 || now.Hour == 13)
                        Program.PostToGeneralChannel(null, Utils.SendBlue("Clan Battle has started!"));
            }
            else
            {
                Program.SetStatus(DSharpPlus.Entities.UserStatus.Idle);
            }
        }
        static double MillisecondsToMinutes(double minutes)
        {
            return minutes * 60000;
        }
    }
}
