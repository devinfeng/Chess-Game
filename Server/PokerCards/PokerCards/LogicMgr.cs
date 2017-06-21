using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerCards._7wang523;

namespace PokerCards
{
    class LogicMgr : ManagerBase<LogicMgr>
    {
        private List<UserInfo> mOnlinePlayers = new List<UserInfo>();


        public void AddUser(UserInfo user)
        {
            if (!mOnlinePlayers.Contains(user))
                mOnlinePlayers.Add(user);
        }

        public void RemoveUser(UserInfo user)
        {
            if (mOnlinePlayers.Contains(user))
                mOnlinePlayers.Remove(user);
        }

        public void RemoveUser(ClientService service)
        {
            for (int i = 0; i < mOnlinePlayers.Count; i++)
            {
                if (mOnlinePlayers[i].mService == service)
                {
                    mOnlinePlayers.RemoveAt(i);
                    break;
                }
            }
        }

        public UserInfo GetUserInfo(int id)
        {
            for (int i = 0; i < mOnlinePlayers.Count; i++)
            {
                if (mOnlinePlayers[i].mID == id)
                    return mOnlinePlayers[i];
            }
            return null;
        }
    }
}
