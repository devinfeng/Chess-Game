using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerCards
{
    static class MessageProtocol
    {
        public static int C2S_Login = 1001;
        public static int S2C_LoginReply = 1002;
        //
        public static int C2S_SingleMatch = 2001;
        public static int S2C_SingleMatchReply = 2002;
        //
        public static int C2S_CreateTeam = 2003;
        public static int S2C_CreateTeamReply = 2004;
        //
        public static int C2S_Invite = 2005;
        public static int S2C_InviteReply = 2006;
        //
        public static int C2S_GetRoomInfo = 2007;
        public static int S2C_GetRoomInfoReply = 2008;
        // CurCardPlayer
        public static int C2S_PlayCard = 2009;
        public static int S2C_PlayCardReply = 2010;
        // CurCardPlayer
        public static int C2S_NoticeNextSend = 2011;
        public static int S2C_NoticeNextSendReply = 2012;
        //  NoticeScore
        public static int C2S_NoticePlayerScore = 2013;
        public static int S2C_NoticePlayerScoreReply = 2014;
        //  PlayCards
        public static int C2S_RefreshCards = 2015;
        public static int S2C_RefreshCardsReply = 2016;
        //  SetCardNum
        public static int C2S_SetEndCardNum = 2017;
        public static int S2C_SetEndCardNumReply = 2018;
        //  NoticeChangeCard
        public static int C2S_ChangeCardNum = 2019;
        public static int S2C_ChangeCardNumReply = 2020;
        //  NoticeChangeCard
        public static int C2S_ChangeCard = 2021;
        public static int S2C_ChangeCardReply = 2022;
    }
}
