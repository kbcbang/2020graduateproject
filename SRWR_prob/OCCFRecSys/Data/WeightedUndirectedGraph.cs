using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;

namespace OCCFRecSys
{
    public class WeightedUndirectedGraph
    {
        public Dictionary<int, SortedSet<Weight>> graph;
        public HashSet<int> fromids;
        public int Edges;
        public int user_size;
        private double MinWeight = double.MaxValue;
        private double MaxWeight = double.MinValue;
        public double[] balance;
        public SparseCompressedColumnMatrix<double> [] AdjancyMatrixs;
        public List<int> dangling_node;
        public List<int> global_p;
        public List<int> global_n;

        /// <summary>
        /// SoRec 함수.
        /// </summary>
        /// <param name="k"># of nearest neighbors</param>
        /// <param name="e">threshold of weight</param>
        /// <param name="num_user"></param>
        public WeightedUndirectedGraph(List<WeightedLink> Links, double e, int num_user, int[] balance)
        {
            int edges = 0;
            user_size = num_user;


            graph = new Dictionary<int, SortedSet<Weight>>();
            fromids = new HashSet<int>();

            // build graph(adjacency list) from links
            foreach (WeightedLink i in Links)
            {
                // if the row not exists (if there is not i.x in fromids)
                if (!fromids.Contains(i.x))
                {
                    fromids.Add(i.x);
                    graph.Add(i.x, new SortedSet<Weight>());
                }

                // find size(last node id) of graph
       

                // 단방향만 삽입 (데이터셋을 양방향처리해놨기에) create undirected edge
                graph[i.x].Add(new Weight(i.y, i.w));
                if (i.w > MaxWeight)
                    MaxWeight = i.w;
                if (i.w < MinWeight)
                    MinWeight = i.w;
                edges++;
            }
            this.Edges = edges;

            double[] temp = new double[balance.Length];
            for (int k = 0; k < balance.Length; k++)
            {
                temp[k] = (double)balance[k] / (double)100;
            }
            this.balance = temp;
        }

        /// <param name="k"># of nearest neighbors</param>
        /// <param name="e">threshold of weight</param>
        public WeightedUndirectedGraph(List<WeightedLink> Links, double e)
        {
            int edges = 0;
            user_size = -1;

            /// e가 weight에 threshold라는데..?

            graph = new Dictionary<int, SortedSet<Weight>>();
            fromids = new HashSet<int>();

            // build graph(adjacency list) from links
            foreach (WeightedLink i in Links)
            {
                // if the row not exists
                if (!fromids.Contains(i.x))
                {
                    fromids.Add(i.x);
                    graph.Add(i.x, new SortedSet<Weight>());
                }
                if (!fromids.Contains(i.y))
                {
                    fromids.Add(i.y);
                    graph.Add(i.y, new SortedSet<Weight>());
                }

                if (user_size < i.x)
                    user_size = i.x;

                graph[i.x].Add(new Weight(i.y, i.w));
                graph[i.y].Add(new Weight(i.x, i.w));
                if (i.w > MaxWeight)
                    MaxWeight = i.w;
                if (i.w < MinWeight)
                    MinWeight = i.w;
                edges++;
            }
            this.Edges = edges;
            this.user_size += 1;

        }

        public WeightedUndirectedGraph(List<WeightedLink> Links)
        {
            int edges = 0;
            user_size = -1;

            graph = new Dictionary<int, SortedSet<Weight>>();
            fromids = new HashSet<int>();

            // build graph(adjacency list) from links
            foreach (WeightedLink i in Links)
            {
                // if the row not exists
                if (!fromids.Contains(i.x))
                {
                    fromids.Add(i.x);
                    graph.Add(i.x, new SortedSet<Weight>());
                }
                if (!fromids.Contains(i.y))
                {
                    fromids.Add(i.y);
                    graph.Add(i.y, new SortedSet<Weight>());
                }

                if (user_size < i.x)
                    user_size = i.x;

                graph[i.x].Add(new Weight(i.y, i.w));
                graph[i.y].Add(new Weight(i.x, i.w));
                if (i.w > MaxWeight)
                    MaxWeight = i.w;
                if (i.w < MinWeight)
                    MinWeight = i.w;
                edges++;
            }
            this.Edges = edges;
            this.user_size += 1;
        }

        public SortedSet<Weight> GetOutlinks(int i)
        {
            if (fromids.Contains(i))
                return graph[i];
            else
                return null;
        }

        public void CreateAdjacencyMatrx()
        {
            // 0:pp / 1:pn / 2:np / 3:nn

            SortedSet<Weight> links;
            int sum = 0;

            // link가 traning set에 존재하지 않은 dangling 노드들
            dangling_node = new List<int>();

            // 스킵으로 인해 global하게 점수를 나눠줄 노드들
            global_p = new List<int>();
            global_n = new List<int>();

            for (int i = 0; i < user_size; i++)
            {
                links = GetOutlinks(i);

                if (links == null)
                {
                    dangling_node.Add(i);
                }
            }

            /*
            for (int i = user_size; i < Size; i++)
            {
                links = GetOutlinks(i);

                if (links == null)
                {
                    dangling_node.Add(i);
                }
            }*/
        }
    }

    public struct WeightedLink
    {
        public WeightedLink(int a, int b, double c)
        {
            x = a;
            y = b;
            w = c;
        }

        public int x;
        public int y;
        public double w;

        public static int CompareWeightedLink(WeightedLink a, WeightedLink b)
        {
            int result = a.w.CompareTo(b.w);

            if (result == 0)
                return a.x.CompareTo(b.x);
            else
                return result;
        }
        public static int CompareWeightedLinkDesc(WeightedLink a, WeightedLink b)
        {
            int result = b.w.CompareTo(a.w);

            if (result == 0)
                return a.x.CompareTo(b.x);
            else
                return result;
        }
    }

    public struct Weight : IComparable
    {
        public Weight(int ID, double Weight)
        {
            id = ID;
            w = Weight;
        }

        public int id;
        public double w;

        public static int CompareWeight(Weight a, Weight b)
        {
            int result = a.w.CompareTo(b.w);

            if (result == 0)
                return a.id.CompareTo(b.id);
            else
                return result;
        }
        public static int CompareWeightDesc(Weight a, Weight b)
        {
            int result = b.w.CompareTo(a.w);

            if (result == 0)
                return a.id.CompareTo(b.id);
            else
                return result;
        }
        public static int CompareID(Weight a, Weight b)
        {
            return a.id.CompareTo(b.id);
        }

        public int CompareTo(object ob)
        {
            Weight obj = (Weight)ob;

            int result = obj.w.CompareTo(this.w);
            if (result == 0)
                return this.id.CompareTo(obj.id);
            else
                return result;
        }

    }
}
