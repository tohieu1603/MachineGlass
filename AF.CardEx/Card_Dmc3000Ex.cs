using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AF.Card;

namespace AF.CardEx
{
    public class Card_Dmc3000Ex : Card_Dmc3000
    {
        public Card_Dmc3000Ex(int nCardTotalIndex, int nCardIndex, string strName)
            : base(nCardTotalIndex, nCardIndex, strName)
        {

        }

        public void Func1()
        {
            Console.WriteLine("Func1");
        }
    }
}
