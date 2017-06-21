using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;
using System.IO;

namespace PokerCards._7wang523
{
    class BattleRoom
    {
        // 当前房间所有的底牌
        private List<CardInfo> curRoomTotalCards = new List<CardInfo>();
        // 当前房间所有玩家的信息 key 坑位 
        private Dictionary<int, RoomPlayer> mPlayers = new Dictionary<int, RoomPlayer>();
        // 当前回合打出的所有牌
        private List<CardInfo> curRoundCards = new List<CardInfo>();
        // 打出上一张或几张牌
        private PlayCards lastCard = new PlayCards();
        private int mRoomID = 0;

        // 牌堆里当前的索引值
        private int curRoomCardIndex = 0;
        // 当前回合庄家
        private int curBankerIndex = 0;
        // 当前出牌的玩家作引
        private int curCardPowerIndex = 0;
        // 当前游戏时间
        private float curGameTime = 0;
        // 当前出牌时间
        private float curCardTime = 0;
        // 最后一起出牌的张数
        private int endCardNum = 0;

        // 游戏开始
        private bool isGameStart = false;
        // 最后一轮
        private bool isInLastTurn = false;
        // 最后一轮每个玩家可持牌数
        private int cardNumInLast = 0;


        public List<int> playerIDList { get; private set; }

        // 注册房间内消息
        private void RegisterMsg()
        {
            GameServer.Instance.RegisterMsg(MessageProtocol.C2S_PlayCard, typeof(PlayCards), DealPlayCard);
            GameServer.Instance.RegisterMsg(MessageProtocol.C2S_SetEndCardNum, typeof(SetCardNum), SetEndCardNum);
        }

        // 注册定时器
        private void RegisterTimer()
        {
            TimerInfo info = new TimerInfo(1f, true, TimerCallback);
            TimeMgr.Instance.AddTimer("7Wang523_" + mRoomID,info);
        }

        // 初始化房间信息  初始约定好一号位玩家为庄家
        public void InitRoom(int roomId, List<int> players)
        {
            mRoomID = roomId;
            playerIDList = players;
            int num = players.Count;
            curRoomTotalCards = CardsManager.Instance.GetTotalCards(num < 3 ? 1 : 2);
            for (int i = 0; i < num; i++)
            {
                RoomPlayer player = new RoomPlayer(players[i],GameType._7Wang523);
                player.mPlayerIndex = i;
                List<CardInfo> temp = new List<CardInfo>();
                for (int j = 0; j < GameDefine._7Wang523.STARTCARDS_7W523; j++)
                {
                    player.AddCard(curRoomTotalCards[i + j * GameDefine._7Wang523.STARTCARDS_7W523]);
                }
                
                temp.ForEach(a => LogMgr.Log(string.Format("玩家：{0} 牌：{1} {2}", i, a.type, a.num)));
                if (!mPlayers.ContainsKey(i))
                {
                    mPlayers.Add(i, player);
                }
            }
            curRoomCardIndex = num * GameDefine._7Wang523.STARTCARDS_7W523 - 1;
            curBankerIndex = CheckFirstDealPlayer();
            curCardPowerIndex = curBankerIndex;
            RegisterMsg();
            RegisterTimer();
            // 通知庄家出牌

        }

        // 找出第一个出牌的玩家
        private int CheckFirstDealPlayer()
        {
            int index = 0;
            for (int i = 0; i < curRoomCardIndex; i++)
            {
                int player = i % GameDefine._7Wang523.STARTCARDS_7W523;
                if (curRoomTotalCards[i].num < curRoomTotalCards[index].num)
                    index = i;
                else if (curRoomTotalCards[i].num == curRoomTotalCards[index].num)
                {
                    if((int)curRoomTotalCards[i].type > (int)curRoomTotalCards[index].type)
                        index = i;
                    else if((int)curRoomTotalCards[i].type == (int)curRoomTotalCards[index].type && player == curBankerIndex)
                        index = i;
                }
            }
            return index;
        }

        // 出牌逻辑
        public void DoSendLogic(int index,List<CardInfo> cards)
        {
            if (index != curBankerIndex)
            {
                LogMgr.Error(string.Format("{0}号房间错误！当前出牌玩家{1},应该是玩家{2}",mRoomID, playerIDList[index], playerIDList[curBankerIndex]));
                return;
            }
            if (lastCard.cards.Count != 0)
            {
                if ((!isInLastTurn && cards.Count != lastCard.cards.Count) || (isInLastTurn && cards.Count != endCardNum))
                {
                    LogMgr.Error(string.Format("{0}号房间玩家{1} 出牌错误,牌的数量不正确！", mRoomID, playerIDList[index]));
                    CurCardPlayer msg = new CurCardPlayer();
                    msg.id = playerIDList[index];
                    msg.codeId = 104;       // 牌的数量不正确
                    SendMsgToPlayer<CurCardPlayer>(playerIDList[curBankerIndex], MessageProtocol.S2C_PlayCardReply, msg);
                    return;
                }
            }
            if (lastCard.cards.Count > 1)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    for (int j = 0; j < cards.Count; j++)
                    {
                        if (cards[i].num != cards[j].num)
                        {
                            LogMgr.Error(string.Format("{0}号房间玩家{1} 出牌错误,多张牌的大小不一样！", mRoomID, playerIDList[index]));
                            CurCardPlayer msg = new CurCardPlayer();
                            msg.id = playerIDList[index];
                            msg.codeId = 105;       // 多张牌必须相同
                            SendMsgToPlayer<CurCardPlayer>(playerIDList[curBankerIndex], MessageProtocol.S2C_PlayCardReply, msg);
                            return;
                        }
                    }
                }
            }
            if (!isInLastTurn && cards[0].weight < lastCard.cards[0].weight)
            {
                LogMgr.Error(string.Format("{0}号房间玩家{1} 出牌错误,出的牌比上一张小！", mRoomID, playerIDList[index]));
                CurCardPlayer msg = new CurCardPlayer();
                msg.id = playerIDList[index];
                msg.codeId = 106;       // 出的牌比上一张小
                SendMsgToPlayer<CurCardPlayer>(playerIDList[curBankerIndex], MessageProtocol.S2C_PlayCardReply, msg);
                return;
            }
            if (!isInLastTurn)
            {
                if (mPlayers[index].RemoveCards(cards))
                {
                    lastCard = new PlayCards();
                    lastCard.id = playerIDList[index];
                    lastCard.cards.AddRange(cards);
                    curRoundCards.AddRange(cards);

                    LogMgr.Log(string.Format("{0}号房间玩家{1} 出牌成功", mRoomID, playerIDList[index]));
                    // 发送出牌成功协议
                    PlayCards msg = new PlayCards();
                    msg.id = playerIDList[index];
                    msg.cards.AddRange(cards);
                    BordcastMsg<PlayCards>(MessageProtocol.S2C_PlayCardReply, msg);
                    ResetBanker();
                }
            }
            else
            {
                if (mPlayers[index].CheckCards(cards))
                {
                    mPlayers[index].mLastCard = new List<CardInfo>();
                    mPlayers[index].mLastCard.AddRange(cards);
                }
            }
        }

        // 设置下一个出牌玩家
        public void ResetBanker()
        {
            if (curCardPowerIndex < playerIDList.Count - 1)
                curCardPowerIndex++;
            else
                curCardPowerIndex = 0;
            if (playerIDList[curCardPowerIndex] == lastCard.id)  //本回合结束，结算
            {
                int score = 0;
                for (int i = 0; i < curRoundCards.Count; i++)
                {
                    if (curRoundCards[i].num == 5)
                        score += 5;
                    else if (curRoundCards[i].num == 10 || curRoundCards[i].num == 13)
                        score += 10;
                }
                // 清空本回合牌堆
                curRoundCards.Clear();
                mPlayers[playerIDList[curCardPowerIndex]].mScore += score;
                curBankerIndex = curCardPowerIndex;     // 设置得分玩家为下一回合庄家
                // 通知玩家得分
                NoticeScore mSco = new NoticeScore();
                mSco.id = playerIDList[curCardPowerIndex];
                mSco.score = score;
                BordcastMsg<NoticeScore>(MessageProtocol.S2C_NoticePlayerScoreReply, mSco);
                CheckIsEnd();
            }
            else
            {
                NoticeNext();
            }
        }

        // 通知下一位玩家出牌
        public void NoticeNext()
        {
            curCardTime = 0;
            CurCardPlayer msg = new CurCardPlayer();
            msg.id = playerIDList[curCardPowerIndex];
            msg.codeId = 1;     // 出牌
            BordcastMsg<CurCardPlayer>(MessageProtocol.S2C_NoticeNextSendReply, msg);
        }

        // 检查是否到达牌局最后阶段
        public void CheckIsEnd()
        {
            int length = 0;
            foreach (var item in mPlayers.Values)
            {
                length += item.PlayerCards.Count;
            }
            int left = curRoomTotalCards.Count - curRoomCardIndex;
            int need = GameDefine._7Wang523.STARTCARDS_7W523 * playerIDList.Count - length;
            if (need > left)   // 到达最后一轮 需要一起出
            {
                // 通知庄家设置出牌数量


                // 补全玩家的手牌 多的去掉 少的随机从牌堆里抽取
                int total = playerIDList.Count;
                cardNumInLast = (int)Math.Floor((left + length) / total * 1.0) + left % total > total / 2 ? 1 : 0;
                if (cardNumInLast > GameDefine._7Wang523.STARTCARDS_7W523)
                {
                    LogMgr.Error("1 计算补全手牌时出现逻辑错误！");
                }
                foreach (var item in mPlayers.Values)
                {
                    int k = item.PlayerCards.Count;
                    item.mChangeCardNum = k - cardNumInLast;
                    if (item.mChangeCardNum > 0)
                    {
                        NoticeChangeCard change = new NoticeChangeCard();
                        change.id = item.userInfo.mID;
                        change.num = item.mChangeCardNum;
                        SendMsgToPlayer<NoticeChangeCard>(change.id, MessageProtocol.S2C_ChangeCardNumReply, change);
                    }
                }

                isInLastTurn = true;
                // 换牌时间
                TimerInfo info = new TimerInfo(GameDefine._7Wang523.CHANGECARDTIME,false,ChangeCardCallback);
                TimeMgr.Instance.AddTimer("ChangeCardTime", info);
            }
            else
            {
                AddEachPlayerCards();
                NoticeNext();
            }
        }

        // 接牌
        public void AddEachPlayerCards()
        {
            // 从庄家开始依次从牌堆里补全玩家手牌
            List<int> temp = new List<int>();
            for (int i = curBankerIndex; i < playerIDList.Count; i++)
            {
                temp.Add(playerIDList[i]);
            }
            for (int i = 0; i < curBankerIndex; i++)
            {
                temp.Add(playerIDList[i]);
            }

            if (temp.Count != playerIDList.Count)
                LogMgr.Error("逻辑错误！");

            for (int m = 0; m < temp.Count; m++)
            {
                if (mPlayers[temp[m]] != null)
                {
                    int count = mPlayers[temp[m]].PlayerCards.Count;
                    for (int i = count; i < GameDefine._7Wang523.STARTCARDS_7W523; i++)
                    {
                        curRoomCardIndex++;
                        mPlayers[temp[m]].AddCard(curRoomTotalCards[curRoomCardIndex]);
                    }
                    // 
                    PlayCards msg = new PlayCards();
                    msg.id = temp[m];
                    msg.cards.AddRange(mPlayers[temp[m]].PlayerCards);
                    SendMsgToPlayer<PlayCards>(temp[m], MessageProtocol.S2C_RefreshCardsReply, msg);
                }
            }
        }

        // 出牌定时回调
        private void TimerCallback()
        {
            if (isGameStart)
            {
                curGameTime++;
                curCardTime++;
                if (!isInLastTurn)
                {
                    if (curCardTime >= mPlayers[curBankerIndex].preCardTime)   // 超时
                    {
                        LogMgr.Warning(string.Format("{0}号房间玩家{1} 出牌超时！", mRoomID, playerIDList[curBankerIndex]));
                        mPlayers[curBankerIndex].mTimeOverTimes++;
                        if (playerIDList[curBankerIndex] != null)
                        {
                            CurCardPlayer msg = new CurCardPlayer();
                            msg.id = playerIDList[curBankerIndex];
                            msg.codeId = 110;       // 超时
                            SendMsgToPlayer<CurCardPlayer>(playerIDList[curBankerIndex], MessageProtocol.S2C_PlayCardReply, msg);
                        }
                        ResetBanker();
                    }
                }
                else   // 在最后一轮
                {
                    if (curCardTime >= GameDefine._7Wang523.CHOOSE_CARDTIME)    // 选牌超时
                    {
                        List<PlayCards> cards = new List<PlayCards>();
                        foreach (var item in mPlayers.Values)
                        {
                            // 如果没有选定，则默认从最小的开始出
                            if(item.mLastCard == null)
                            {
                                item.mLastCard = new List<CardInfo>();
                                for (int i = 0; i < endCardNum; i++)
                                {
                                    item.mLastCard.Add(item.PlayerCards[item.PlayerCards.Count - 1]);
                                }
                            }
                            // 出牌
                            item.RemoveCards(item.mLastCard);
                            // 广播消息
                            PlayCards card = new PlayCards();
                            card.id = item.userInfo.mID;
                            card.cards.AddRange(item.mLastCard);
                            BordcastMsg<PlayCards>(MessageProtocol.S2C_PlayCardReply, card);
                            cards.Add(card);
                            item.mLastCard = null;
                        }
                        cards.Sort(Compare);
                        int score = 0;
                        for (int i = 0; i < cards.Count; i++)
                        {
                            if (cards[i].cards[0].num == 5)
                                score += 5 * endCardNum;
                            else if (cards[i].cards[0].num == 10 || cards[i].cards[0].num == 13)
                                score += 10 * endCardNum;
                        }
                        curCardPowerIndex = playerIDList.IndexOf(cards[0].id);
                        if (curCardPowerIndex != -1)
                        {
                            mPlayers[playerIDList[curCardPowerIndex]].mScore += score;
                            curBankerIndex = curCardPowerIndex;     // 设置得分玩家为下一回合庄家
                            // 通知玩家得分
                            NoticeScore mSco = new NoticeScore();
                            mSco.id = playerIDList[curCardPowerIndex];
                            mSco.score = score;
                            BordcastMsg<NoticeScore>(MessageProtocol.S2C_NoticePlayerScoreReply, mSco);
                            if (mPlayers[playerIDList[curCardPowerIndex]].PlayerCards.Count < 1)
                            {
                                // 游戏结束  根据玩家分数结算

                            }
                            else
                            {
                                // 本轮一起亮牌结束 通知庄家设置数量
                                CurCardPlayer msg = new CurCardPlayer();
                                msg.id = playerIDList[curCardPowerIndex];
                                msg.codeId = 2;
                                BordcastMsg<CurCardPlayer>(MessageProtocol.S2C_NoticeNextSendReply, msg);
                            }
                        }
                    }
                }
            }
        }

        // 最后一轮比较牌的大小
        private int Compare(PlayCards x, PlayCards y)
        {
            if (x.cards.Count == 1)
            {
                if (x.cards[0].weight > y.cards[0].weight)
                    return 1;
                else if (x.cards[0].weight == y.cards[0].weight)
                {
                    if((int)x.cards[0].type > (int)x.cards[0].type)
                        return 1;
                    else if((int)x.cards[0].type == (int)x.cards[0].type)
                    {
                        int a = playerIDList.IndexOf(x.id);
                        int b = playerIDList.IndexOf(y.id);
                        if (a == curBankerIndex) return 1;
                        if (b == curBankerIndex) return 1;
                        if (a > b) return 1;
                    }
                }
            }
            else
            {
                if (x.cards[0].weight > y.cards[0].weight)
                    return 1;
                else if (x.cards[0].weight == y.cards[0].weight)
                {
                    int a = playerIDList.IndexOf(x.id);
                    int b = playerIDList.IndexOf(y.id);
                    if (a == curBankerIndex) return 1;
                    if (b == curBankerIndex) return 1;
                    if (a > b) return 1;
                }
            }
            return -1;
        }

        // 换牌定时回调
        private void ChangeCardCallback()
        {
            foreach (var item in mPlayers.Values)
            {
                int k = item.PlayerCards.Count;
                while (k > cardNumInLast)
                {
                    curRoomTotalCards.Add(item.PlayerCards[k - 1]);
                    item.RemoveCardByIndex(k - 1);
                    k = item.PlayerCards.Count;
                }
            }
            int left = curRoomTotalCards.Count - curRoomCardIndex;
            for (int i = 0; i < cardNumInLast * playerIDList.Count - left; i++)
            {
                Random ran = new Random();
                int idx = ran.Next(0, curRoomCardIndex);
                curRoomTotalCards.Add(curRoomTotalCards[idx]);
            }
            AddEachPlayerCards();
            // 换牌结束，庄家决定出牌数量
            CurCardPlayer msg = new CurCardPlayer();
            msg.id = playerIDList[curCardPowerIndex];
            msg.codeId = 2;     // 需要一起出牌 请决定出牌张数
            BordcastMsg<CurCardPlayer>(MessageProtocol.S2C_NoticeNextSendReply, msg);
        }

        //************************************************************ 发送消息 *************************************************************

        // 房间内广播消息
        public void BordcastMsg<T>(int msgID, T msg)
        {
            for (int i = 0; i < playerIDList.Count; i++)
            {
                UserInfo info = LogicMgr.Instance.GetUserInfo(playerIDList[i]);
                GameServer.Instance.SendMsg<T>(info.mService, msgID, msg);
            }
        }

        // 给某位玩家发送消息
        public void SendMsgToPlayer<T>(int player, int msgId, T msg)
        {
            UserInfo info = LogicMgr.Instance.GetUserInfo(player);
            GameServer.Instance.SendMsg<T>(info.mService, msgId, msg);
        }


        //************************************************************ 接收消息 *************************************************************

        // 只剩最后一轮手牌时,庄家设置出牌张数
        public void SetEndCardNum(object msg)
        {
            SetCardNum m = (SetCardNum)msg;
            if(m.num > mPlayers[m.id].PlayerCards.Count)
            {
                LogMgr.Error(string.Format("{0}号房间玩家{1} 设置出牌数量错误！",mRoomID,m.id));
                return;
            }
            endCardNum = m.num;

            // 广播给所有玩家
            SetCardNum sen = new SetCardNum();
            sen.num = endCardNum;
            BordcastMsg<SetCardNum>(MessageProtocol.S2C_SetEndCardNumReply, sen);
        }

        // 接受到玩家出牌消息
        public void DealPlayCard(object msg)
        {
            PlayCards card = (PlayCards)msg;
            int index = playerIDList.IndexOf(card.id);
            if (index != -1)
                DoSendLogic(index, card.cards);
            else
                LogMgr.Error("玩家不在游戏中,不应该接受到此消息...");
        }

        // 接收玩家使用道具换牌
        public void ReceivePlayerChangeCard(object msg)
        {
            NoticeChangeCard change = (NoticeChangeCard)msg;
            if (mPlayers[change.id] != null && mPlayers[change.id].PlayerCards.Count < change.num)
            {
                mPlayers[change.id].RemoveCards(change.cards);
                curRoomTotalCards.AddRange(change.cards);
            }
        }
    }
}
