using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerCards
{
    class LogMgr
    {
        private static bool isOpenLog = true;
        private static ConsoleColor orginColor = Console.ForegroundColor;

        public static void Log(string info)
        {
            if (!isOpenLog)
                return;
            Console.ForegroundColor = orginColor;
            info = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss-ffff  ") + info;
            Console.WriteLine(info);
        }

        public static void Warning(string msg)
        {
            if (!isOpenLog)
                return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss-ffff  ") + msg;
            Console.WriteLine(msg);
        }

        public static void Error(string msg)
        {
            if (!isOpenLog)
                return;
            Console.ForegroundColor = ConsoleColor.Red;
            msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss-ffff  ") + msg;
            Console.WriteLine(msg);
        }
    }
}
