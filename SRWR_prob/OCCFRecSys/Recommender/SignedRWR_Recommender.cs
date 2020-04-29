using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extreme.Mathematics;
using System.Diagnostics;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;

namespace OCCFRecSys
{
    public class SignedRWR_Recommender : Recommender
    {
        int fold;
        string candidate_item;
        // RWR variables
        double e, damping_factor;
        public int user_size, num_neighbor;                             // graph의 user 수, graph 생성 시 파라미터 (이웃 수)
        double beta, gamma, delta;
        public int num_user;
        List<WeightedLink> links;                                       // graph의 links
        ConcurrentDictionary<int, List<Score>> recommend_user_item;               // 타겟 유저들에게 추천할 아이템들과 점수 저장
        int[] balance;

        // SoRec 함수
        public SignedRWR_Recommender(string candidate_item, int num_recommend, int fold, StreamReader test, double beta, double gamma, double delta, int num_user,
            int[] balance, StreamReader social = null, StreamReader social_unint = null, StreamReader training = null, StreamReader training_unint = null)
        {
            this.candidate_item = candidate_item;
            this.fold = fold;

            /// 이거는 예전 SRWR에서 쓰던거
            this.beta = beta;
            this.gamma = gamma;
            this.delta = delta;

            this.num_user = num_user;
            Init_SoRec(num_recommend, test, balance, social, social_unint, training, training_unint);
        }

        public SignedRWR_Recommender(string candidate_item, int num_recommend, int fold, StreamReader training, StreamReader test, double beta, double gamma, int num_user)
        {
            this.candidate_item = candidate_item;
            this.fold = fold;
            this.beta = beta;
            this.gamma = gamma;
            this.num_user = num_user;

            Init(num_recommend, training, test);
        }

        // SoRec
        public void Init_SoRec(int num_recommend, StreamReader test, int[] balance, StreamReader social, StreamReader social_unint, StreamReader training, 
            StreamReader training_unint)
        {
            // RWR variables
            e = 0.0d; // 안씀 
            damping_factor = 0.85d; // restart 확률 

            this.num_recommend = num_recommend;

            // test set 읽기
            ReadTestSet(test, candidate_item);
            test.Close();
            Console.WriteLine("Read test set: Complete");

            links = new List<WeightedLink>();
            this.balance = balance;

            if (social != null)
                // social set 읽기
                ReadTrainingSet_SoRec(social);
            if (social_unint != null)
                // social set 읽기
                ReadTrainingSet_SoRec(social_unint);
            if (training != null)
                // training set 읽기
                ReadTrainingSet_SoRec(training);
            if (training_unint != null)
                // training set 읽기
                ReadTrainingSet_SoRec(training_unint);

            training.Close();
            Console.WriteLine("Read training set: Complete");
        }

        /// <summary>
        /// dataset과 parameter 설정. user-item pair.
        /// </summary>
        /// <param name="num_recommend"></param>
        /// <param name="training"></param>
        /// <param name="test"></param>
        public override void Init(int num_recommend, StreamReader training, StreamReader test)
        {
            // RWR variables
            e = 0.0d;
            damping_factor = 0.85d;
            this.num_recommend = num_recommend;

            // test set 읽기
            ReadTestSet(test, candidate_item);
            test.Close();
            Console.WriteLine("Read test set: Complete");

            // training set 읽기

            ReadTrainingSet(training);
            training.Close();
            Console.WriteLine("Read training set: Complete");
        }

        public void ReadTrainingSet_SoRec(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int user_id = int.Parse(line[0].ToString());
                // int item_id = int.Parse(line[1].ToString()) + Program.num_user;
                int item_id = int.Parse(line[1].ToString());
                int rating = (int)double.Parse(line[2].ToString());
                if (rating == 0)
                    rating = -1;

                links.Add(new WeightedLink(user_id, item_id, rating)); // weight 추가시: double.Parse(line[2])
            }
        }

        public override void ReadTrainingSet(StreamReader sr)
        {
            links = new List<WeightedLink>();

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int user_id = int.Parse(line[0].ToString());
                int item_id = int.Parse(line[1].ToString());
                int rating = int.Parse(line[2].ToString());
                if (rating == 0)
                    rating = -1;

                links.Add(new WeightedLink(user_id, item_id, rating)); // weight 추가시: double.Parse(line[2])
            }
        }

        public override void Recommend()
        {
            // RWR 점수 계산
            recommend_user_item = new ConcurrentDictionary<int, List<Score>>();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // 그래프 모델링 및 분석
            WeightedUndirectedGraph model_graph = new WeightedUndirectedGraph(links, e, num_user, balance);
            sw.Stop();

            Console.WriteLine("Complete 'links to graph'");
            TimeSpan ts = sw.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime : " + elapsedTime);

            user_size = model_graph.user_size;
            sw.Start();

            model_graph.CreateAdjacencyMatrx();
            sw.Stop();
            Console.WriteLine("Complete 'graph to matrix'");
            ts = sw.Elapsed;

            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime : " + elapsedTime);
            Console.WriteLine("dangling node's Count: {0}", model_graph.dangling_node.Count);

            Console.WriteLine("global_p's count: {0}", model_graph.global_p.Count);
            Console.WriteLine("global_n's count: {0}", model_graph.global_n.Count);

            sw.Start();

            int testCount = correct_user_item.Keys.Count();
            int ff = 0;
            Parallel.For(0, user_size, (i) =>
            {
                if (correct_user_item.ContainsKey(i))
                {
                    Interlocked.Increment(ref ff);
                    Console.WriteLine("user id " + i + " 's rec started, ("  + ff + "/" + testCount + ")");
                    List<Score> recommend_items = PerformRWR(model_graph, i);
                    recommend_user_item.TryAdd(i, recommend_items);
                }
            });



            Console.WriteLine("Compute RWR scores of {0}: Complete", fold);
            sw.Stop();
            ts = sw.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime : " + elapsedTime);
        }

        /// <summary>
        /// user-item 형태로 모델링된 bipartite graph를 사용하여 모든 test user에 대해 RWR 수행 
        /// </summary>
        /// <param name="model_graph">user-item에 대한 bipartite graph</param>
        /// <param name="sw">추천 결과(사용자 id, 아이템 id, 점수)가 출력될 파일</param>
        public List<Score> PerformRWR(WeightedUndirectedGraph model_graph, int target)
        { 
            // test user가 기존에 갖고 있는 이력 리스트를 저장
            HashSet<int> i_items = new HashSet<int>();
            if (model_graph.graph.ContainsKey(target))
            {
                SortedSet<Weight> i_weight = model_graph.graph[target];
                foreach (Weight items in i_weight)
                    i_items.Add(items.id);
            }
            // RWR 수행
            SignedRWR ranker = new SignedRWR(model_graph, damping_factor, target, beta, gamma, delta);
            List<int> score = ranker.Calculate();

            // RWR 점수를 기준으로 타겟 사용자가 선호할 만한 TOP N개의 아이템 출력
            List<Score> recommend_items = new List<Score>();
            //Dictionary<int, double> recommend_items = new Dictionary<int, double>();
            /*foreach (Weight s in score)
            {
                // ID가 user ID 이거나, 추천 아이템이 타겟 사용자의 기존 이력에 포함되어 있으면 TOP N에 포함시키지 않음  
                if (s.id < user_size || i_items.Contains(s.id)) continue;
                else if (recommend_items.Count < num_recommend)
                {
                    if (candidate_item == "Longtail_items")
                        if (Program.tophead_items.Contains(s.id)) continue;

                    Score user_score = new Score(s.id, s.w);
                    recommend_items.Add(user_score);
                }
                else
                    break;
            }*/

            return recommend_items;
        }

        public override void PrintResults(StreamWriter sw)
        {
            Dictionary<int, List<Score>> prdict = recommend_user_item.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ComputeMRR(prdict, sw);
            ComputeHLU(prdict, sw);
            ComputeAccuary(prdict, sw);
            ComputeNDCG(prdict, sw);
            Console.WriteLine("Compute accuracy of recommendation: Complete");
        }

    }
}
