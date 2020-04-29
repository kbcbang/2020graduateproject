using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace OCCFRecSys
{
    class Program
    {
        public static string file_path;
        public static StreamReader sr;
        public static StreamWriter sw;
        public static HashSet<int> tophead_items;
        public static int num_user;
        //public static double[] betas = { 0.9 };
        //public static double[] gammas = { 0.9 };
        //public static double beta = 0d;
        //public static double gamma = 0d;

        static void Main(string[] args)
        {
            // r and edge's sign이 +가 될 확률: u-u (++, +-/-+, --) / u-i (++, +-, -+, --) / i-u (++, +-/-+, --)
            //int[] balance = new int[10] { 100, 0, 100, 100, 0, 100, 100, 100, 0, 100 };
            int beta = 80; // --+
            int gamma = 60; //-+-
            int[] SRWR_balance = new int[4] { 100, 100, gamma, beta };
            
            //balance = new int[10] { 91, 51, 87, 95, 0, 0, 100, 100, 0, 100 };
            
            run("Lastfm", true, true, true, true, SRWR_balance, "prob");
            // balance = new int[10] { 100, 50, 100, 100, 0, 0, 100, 100, 0, 100 };
            // run("Lastfm", true, true, true, true, balance, "prob_1");
        }

        public static void run(string dataset, bool b_social_int, bool b_social_unint, bool b_rating_int, bool b_rating_unint, int[] balance, string version)
        {
            string dataset_name;

            switch (dataset)
            {
                /// 여기에 위키 데이터셋 만들어야함. 


                case "Delicious":
                    dataset_name = "deli";
                    num_user = 1867;
                    break;
                case "Epinions":
                    dataset_name = "epi";
                    num_user = 49289;
                    break;
                case "Epinions_extend":
                    dataset_name = "epiex";
                    num_user = 179953;
                    break;
                case "Ciaodvd":
                    dataset_name = "ciao";
                    num_user = 19533;
                    break;
                case "Lastfm":
                    dataset_name = "last";
                    num_user = 1892;
                    break;
                default:
                    Console.WriteLine("Error!!!!");
                    return;
            }

            string[] candidate_items = { "All_items" }; //  "All_items", "Longtail_items"
            string[] algorithms = { "SignedRWR_SoRec" }; //"RWR", "SeparateRWR", "SignedRWR", "SeparateBP", "SignedBP"

            //file_path = "D:\\Downloads\\DataFile\\gOCCF\\Movielens\\";
            //num_user = 943;
            //foreach (string algo in algorithms)
            //    foreach (string candidate_item in candidate_items)
            //        Experiments(algo, candidate_item);
            /*
            file_path = "D:\\Downloads\\DataFile\\gOCCF\\Watcha\\";
            num_user = 1391;
            foreach (string algo in algorithms)
                foreach (string candidate_item in candidate_items)
                    Experiments(algo, candidate_item);
            */

            
            file_path = "C:\\Users\\ddolg\\Desktop\\SRWR\\SRWR_prob\\OCCFRecSys\\Dataset\\" + dataset + "\\";

            foreach (string algo in algorithms)
                foreach (string candidate_item in candidate_items)
                    Experiments(algo, candidate_item, dataset_name, b_social_int, b_social_unint, b_rating_int, b_rating_unint, balance, version);

            //file_path = "D:\\Downloads\\DataFile\\Ciao\\";
            //num_user = 996;
            //foreach (string algo in algorithms)
            //    foreach (string candidate_item in candidate_items)
            //        Experiments(algo, candidate_item);

            //file_path = "D:\\Downloads\\DataFile\\CiteULike\\";
            //num_user = 5551;
            //foreach (string algo in algorithms)
            //    foreach (string candidate_item in candidate_items)
            //        Experiments(algo, candidate_item);


            //foreach (double beta in betas)
            //    foreach (double gamma in gammas)
            //        foreach (string algo in algorithms)
            //            foreach (string candidate_item in candidate_items)
            //            {
            //                Program.beta = beta;
            //                Program.gamma = gamma;
            //                Experiments(algo, candidate_item);
            //            }
        }

        public static void Experiments(string algo, string candidate_item, string dataset_name, bool b_social_int, bool b_social_unint, bool b_rating_int, bool b_rating_unint, int[] balance, string version)
        {
            int num_recommend = 50;

            for (int fold = 1; fold <= 1; fold++)
            {
                if (candidate_item == "Longtail_items")
                {
                    tophead_items = new HashSet<int>();
                    sr = new StreamReader(file_path + "raw\\longtail\\u" + fold + "_longtail_items.txt");

                    while (!sr.EndOfStream)
                    {
                        int item_id = int.Parse(sr.ReadLine().ToString()) + num_user;
                        tophead_items.Add(item_id);
                    }
                    sr.Close();
                }

                /*
                StreamReader training = new StreamReader(file_path + "unint\\basic\\u" + fold + "\\u" + fold + "_balance.base");
                StreamReader test = new StreamReader(file_path + "raw\\basic\\u" + fold + "\\u" + fold + ".test");
                StreamWriter result = new StreamWriter(file_path + "results\\" + algo + "\\unint\\origin\\u" + fold + "_" + algo + "_rankresult_balance(" + candidate_item + ").results");
                StreamWriter time = new StreamWriter(file_path + "results\\" + algo + "\\unint\\origin\\u" + fold + "_" + algo + "_rankresult_balance(" + candidate_item + ").time");
                */

                string data_name = "SRWR_" + dataset_name;
                string result_name = data_name + "_" + version;
             
                if (b_social_int == true)
                {
                    if (b_social_unint == false)
                    {
                        result_name = result_name + "_social(int)";
                    } else
                    {
                        result_name = result_name + "_social(int,unint)";
                    }
                } else 
                {
                    if (b_social_unint == false)
                    {
                    }
                    else
                    {
                        result_name = result_name + "_social(unint)";
                    }
                }
                if (b_rating_int == true)
                {
                    if (b_rating_unint == false)
                    {
                        result_name = result_name + "_rating(int)";
                    }
                    else
                    {
                        result_name = result_name + "_rating(int,unint)";
                    }
                }
                else
                {
                    if (b_rating_unint == false)
                    {
                    }
                    else
                    {
                        result_name = result_name + "_rating(unint)";
                    }
                }

                StreamReader social = new StreamReader(file_path + data_name + "_social_bi.txt");
                StreamReader social_unint = new StreamReader(file_path + data_name + "_social_predict_500.txt");

                StreamReader training = new StreamReader(file_path + data_name + "_rating_train_bi.txt");
                StreamReader training_unint = new StreamReader(file_path + data_name + "_rating_train_predict_500.txt");

                StreamWriter result = new StreamWriter(file_path + "Result\\" + result_name + ".results");
                StreamReader test = new StreamReader(file_path + data_name + "_rating_test.txt");
                StreamWriter time = new StreamWriter(file_path + "Result\\" + result_name + ".time");

                if (b_social_int == false)
                    social = null;
                if (b_social_unint == false)
                    social_unint = null;
                if (b_rating_int == false)
                    training = null;
                if (b_rating_unint == false)
                    training_unint = null;

                Stopwatch sw = new Stopwatch();
                sw.Start();

                if (algo == "SeparateRWR")
                {
                    var recsys = new SeparateRWR_Recommender(candidate_item, num_recommend, fold, training, test);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
               
                else if (algo == "SignedRWR")
                {
                    double beta = 0.9d;
                    double gamma = 0.9d;
                    var recsys = new SignedRWR_Recommender(candidate_item, num_recommend, fold, training, test, beta, gamma, num_user);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                else if (algo == "SignedRWR_SoRec")
                {
                    double beta = 0.9d;
                    double gamma = 0.9d;
                    double delta = 0.5d;
                    
                    var recsys = new SignedRWR_Recommender(candidate_item, num_recommend, fold, test, beta, gamma, delta, num_user, balance, social, social_unint, training, training_unint);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                else if (algo == "SeparateBP")
                {
                    int num_iter = 5;
                    double propagation_alpha = 0.0001d;
                    var recsys = new SeparateBP_Recommender(candidate_item, num_iter, num_recommend, propagation_alpha, fold, training, test);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                else if (algo == "SignedBP")
                {
                    int num_iter = 5;
                    double propagation_alpha = 0.0001d;
                    var recsys = new SignedBP_Recommender(candidate_item, num_iter, num_recommend, propagation_alpha, fold, training, test);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                else if (algo == "RWR")
                {
                    var recsys = new RWR_Recommender(candidate_item, num_recommend, fold, training, test);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                sw.Stop();

                time.WriteLine(algo + "\t" + sw.ElapsedMilliseconds.ToString() + "ms");
                Console.WriteLine(algo + "\t" + sw.ElapsedMilliseconds.ToString() + "ms");

                time.Close();
                result.Close();
            }
        }
    }

    public class Pairs
    {
        public Pairs(int user_id, int item_id, double score)
        {
            this.user_id = user_id;
            this.item_id = item_id;
            this.score = score;
        }

        public int user_id;
        public int item_id;
        public double score;

        public static int CompareScore(Pairs a, Pairs b)
        {
            int result = a.score.CompareTo(b.score);

            return result;
        }
    }



}