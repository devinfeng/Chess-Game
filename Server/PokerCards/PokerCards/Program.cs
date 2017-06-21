using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PokerCards._7wang523;
using Protocol;

namespace PokerCards
{
    class Program
    {
        static void Main(string[] args)
        {
            //BattleRoom room = new BattleRoom();
            //List<int> player = new List<int>();
            //player.Add(1);
            //player.Add(2);
            //room.InitRoom(player);
            GameServer.Instance.Init();
            GameServer.Instance.RegisterMsg(101, typeof(CardInfo), new Message().Print);
            Console.Read();            
        }

        class Message
        {
            public void Print(object param)
            {
                CardInfo info = (CardInfo)param;
                Console.WriteLine("info.num  " + info.num);
                Console.WriteLine("info.weight  " + info.weight);
            }
        }
    }
}
