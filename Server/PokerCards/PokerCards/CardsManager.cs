using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace PokerCards
{
    class CardsManager : ManagerBase<CardsManager>
    {
        private List<CardInfo> TotalCards = new List<CardInfo>();

        public string[] cardColor = new string[] { "黑桃", "红桃", "梅花", "方块", "大王", "小王" };


        // 获取洗好的几幅牌的集合，每次获取都不一样
        public List<CardInfo> GetTotalCards(int num)
        {
            CreateCards(num);
            return TotalCards;
        }

        private void CreateCards(int num)
        {
            List<CardInfo> temp = new List<CardInfo>();
            for (int i = 0; i < 13; i++ )
            {
                int cardNum = i + 1;
                for (int j = 0; j < 4; j++ )
                {
                    CardInfo card = new CardInfo();
                    card.num = cardNum;
                    card.type = (CardType)j;
                    int weight = 0;
                    if (cardNum == 7)
                        weight = 40;
                    else if (cardNum == 5)
                        weight = 30;
                    else if (cardNum == 2)
                        weight = 25;
                    else if (cardNum == 3)
                        weight = 20;
                    else if (cardNum == 1)
                        weight = 15;
                    else
                        weight = cardNum;
                    card.weight = weight;
                    temp.Add(card);
                }
            }
            CardInfo po = new CardInfo();
            po.num = 100;
            po.type = CardType.PowerW;
            po.weight = 35;
            temp.Add(po);
            po = new CardInfo();
            po.num = 50;
            po.type = CardType.SmallW;
            po.weight = 35;
            temp.Add(po);

            TotalCards.Clear();
            for (int i = 0; i < num; i++ )
            {
                TotalCards.AddRange(temp);
            }

            Random random = new Random();
            int count = TotalCards.Count;
            for (int i = 0; i < count; i++)
            {
                int rand = random.Next(0, count);
                if (rand != i)
                {
                    CardInfo tmp = TotalCards[i];
                    TotalCards[i] = TotalCards[rand];
                    TotalCards[rand] = tmp;
                }
            }
            //for (int i = 0; i < count; i++)
            //{
            //    Console.WriteLine(string.Format("牌号：{0} 牌：{1} {2}", i + 1, TotalCards[i].type, TotalCards[i].num));
            //}
        }

    }
}
