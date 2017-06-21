using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerCards._7wang523
{
    class RoomManager : ManagerBase<RoomManager>
    {
        private const int maxRoomLength = 100;
        // 房间列表 key房间号
        private Dictionary<int,BattleRoom> mRoomList = new Dictionary<int,BattleRoom>();
        // 当前匹配排队列表
        private List<int> waitingList = new List<int>();

        

        private int GetRoomID()
        {
            for (int i = 0; i < maxRoomLength; i++)
            {
                if (!mRoomList.ContainsKey(i))
                    return i;
            }
            return -1;
        }

        // 匹配 
        private int MatchRule()
        {

            return 1;
        }

        // 多人组队
        public bool CreateRoomByGroup(List<int> players)
        {
            int id = GetRoomID();
            if (id == -1)
            {

                return false;
            }
            BattleRoom room = new BattleRoom();
            room.InitRoom(id,players);
            mRoomList.Add(id, room);
            return false;
        }

        // 单人匹配
        public bool CreateRoomBySingle(int player)
        {
            if (waitingList.Contains(player))   // 已在匹配队列
            {

            }
            else    
            {
                waitingList.Add(player);
                // 匹配规则

            }
            return false;
        }

        // 和机器人对战
        public bool CreateRoomByRobot(int player,int num)
        {

            return false;
        }

        // 删除房间
        public bool DeleteRoomByID(int roomId)
        {
            if (mRoomList.ContainsKey(roomId))
            {
                mRoomList.Remove(roomId);
                return true;
            }
            return false;
        }
    }
}
