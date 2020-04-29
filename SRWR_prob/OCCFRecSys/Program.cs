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
                case "Wikivote":
                    dataset_name = "wiki";
                    break;
                case "Slashdot":
                    dataset_name = "slashdot";
                    break;
                default:
                    Console.WriteLine("Error!!!!");
                    return;
            }

            string[] candidate_items = { "All_items" }; //  "All_items", "Longtail_items"
            string[] algorithms = { "SignedRWR_SoRec" }; //"RWR", "SeparateRWR", "SignedRWR", "SeparateBP", "SignedBP"
       
            file_path = "C:\\Users\\kbcba\\2020graduateproject\\SRWR_prob\\" + dataset + "\\";

            foreach (string algo in algorithms)
                foreach (string candidate_item in candidate_items)
                    Experiments(algo, candidate_item, dataset_name, b_social_int, b_social_unint, b_rating_int, b_rating_unint, balance, version);

        }

        public static void Experiments(string algo, string candidate_item, string dataset_name, bool b_social_int, bool b_social_unint, bool b_rating_int, bool b_rating_unint, int[] balance, string version)
        {
            int num_recommend = 50;

            for (int fold = 1; fold <= 1; fold++)
            {
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


                if (algo == "SignedRWR_SoRec")
                {
                    double beta = 0.9d;
                    double gamma = 0.9d;
                    double delta = 0.5d;

                    var recsys = new SignedRWR_Recommender(candidate_item, num_recommend, fold, test, beta, gamma, delta, num_user, balance, social, social_unint, training, training_unint);
                    recsys.Recommend();
                    recsys.PrintResults(result);
                }
                else
                    break;
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