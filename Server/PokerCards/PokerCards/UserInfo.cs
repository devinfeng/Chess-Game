using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerCards._7wang523;

namespace PokerCards
{
    public enum GameType
    {
        None,
        _7Wang523,
        YingW,
    }

    class UserInfo
    {
        public int mID { get; private set; }
        public int mName { get; private set; }
        public ClientService mService { get; private set; }

        public GameType mGameState = GameType.None;

        public UserInfo(int id,ClientService service)
        {
            mID = id;
            mName = mName;
            mService = service;
            LogicMgr.Instance.AddUser(this);
        }
    }
}
