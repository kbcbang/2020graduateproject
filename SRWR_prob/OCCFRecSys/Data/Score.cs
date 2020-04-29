using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCCFRecSys
{
    public class Score
    {
        public Score(int item_id, double score)
        {
            this.item_id = item_id;
            this.score = score;
        }

        public int item_id;
        public double score;


        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>


        public static int CompareScore(Score a, Score b)
        {
            /// b<a 
            return b.score.CompareTo(a.score);
        }
    }
}
