using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace PokerCards._7wang523
{
    class RoomPlayer
    {
        public UserInfo userInfo { get; set; }
        public int mPlayerIndex { get; set; }
        public bool isRobot { get; set; }
        public int mScore { get; set; }
        public int mTimeOverTimes { get; set; }
        public int mChangeCardNum { get; set; }
        // 最后一轮选择的牌
        public List<CardInfo> mLastCard { get; set; }

        public float preCardTime
        {
            get
            {
                if (mTimeOverTimes < 3)
                    preCardTime = GameDefine._7Wang523.PRE_CARDTIME;
                else
                    preCardTime = GameDefine._7Wang523.PRE_CARDTIME - 2;
                return preCardTime;
            }
            private set;
        }

        private List<CardInfo> mPlayerCards = new List<CardInfo>();

        public List<CardInfo> PlayerCards
        {
            get 
            {
                return mPlayerCards;
            }
        }

        public RoomPlayer(int id,GameType type)
        {
            userInfo = LogicMgr.Instance.GetUserInfo(id);
            userInfo.mGameState = type;
        }

        public bool CheckCards(List<CardInfo> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (!mPlayerCards.Contains(cards[i]))
                {
                    LogMgr.Error(string.Format("出牌错误，玩家不存在此牌！{0}{1}", CardsManager.Instance.cardColor[(int)cards[i].type], cards[i].num));
                    return false;
                }
            }
            return true;
        }

        // 摸牌
        public void AddCard(CardInfo card)
        {
            mPlayerCards.Add(card);
            // 排序
            mPlayerCards = mPlayerCards.OrderByDescending(a => a.weight).ToList();
        }

        public bool RemoveCardByIndex(int idx)
        {
            if(idx >= mPlayerCards.Count)
            {
                LogMgr.Error("逻辑错误");
                return false;
            }
            mPlayerCards.RemoveAt(idx);
            return true;
        }

        // 出牌
        public bool RemoveCards(List<CardInfo> cards)
        {
            if (CheckCards(cards))
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    mPlayerCards.Remove(cards[i]);
                }
                return true;
            }
            return false;
        }
    }
}
