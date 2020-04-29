using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;

namespace OCCFRecSys
{
    public class SignedRWR
    {
        private double[] score;
        public double[] pscore;
        public double[] nscore;
        private double D;
        private double T = -1.0d;
        private int I = 20;
        private WeightedUndirectedGraph graph;
        public int init_node;
        private double beta;
        private double gamma;
        private double delta;
        public int user_size;
        public int[] cs;
        public double u_global_val;
        public double i_global_val;

        /// <param name="TargetGraph">
        /// Undirected web graph where the RWR will be calculated.
        /// </param>
        /// <param name="DampingFactor">
        /// The damping factor alpha.
        /// </param>
        /// <param name="InitialNode">
        /// List of initial nodes whose initial score set to be 1 and all others 0.
        /// </param>
        public SignedRWR(WeightedUndirectedGraph TargetGraph, double DampingFactor, int InitialNode, double beta, double gamma, double delta)
            : this(TargetGraph, DampingFactor, InitialNode, -1, beta, gamma, delta)
        {
        }

        /// <param name="TargetGraph">
        /// Undirected web graph where the RWR will be calculated.
        /// </param>
        /// <param name="DampingFactor">
        /// The damping factor alpha.
        /// </param>
        /// <param name="InitialNode">
        /// List of initial nodes whose initial score set to be 1 and all others 0.
        /// </param>
        /// <param name="T">
        /// Error tolerance.
        /// </param>
        public SignedRWR(WeightedUndirectedGraph TargetGraph, double DampingFactor, int InitialNode, double T, double beta, double gamma, double delta)
        {
            this.D = DampingFactor;
            this.graph = TargetGraph;
            this.T = T;
            this.beta = beta;
            this.gamma = gamma;
            this.delta = delta;
            this.user_size = TargetGraph.user_size;

            this.cs = new int[8];
            for (int i = 0; i < 8; i++)
            {
                cs[i] = 0;
            }

            score = new double[graph.user_size];

            pscore = new double[graph.user_size];
            nscore = new double[graph.user_size];

            //pscore = Vector.Create<double>(graph.Size);
            //nscore = Vector.Create<double>(graph.Size);
            init_node = InitialNode;
            for (int i = 0; i < score.Length; i++)
            {
                if (InitialNode == i)
                {
                    pscore[i] = 1.0d;
                    nscore[i] = 0.0d;
                    score[i] = pscore[i] - nscore[i];
                }
                else
                {
                    pscore[i] = 0.0d;
                    nscore[i] = 0.0d;
                    score[i] = pscore[i] - nscore[i];
                }
            }

            u_global_val = 1 / (double)(2 * graph.user_size);
            //i_global_val = 1 / (double)(2 * this.user_size);
        }

        public List<int> Calculate()
        {
            double stderr = double.MaxValue;
            int cnt = 1;
      
            //Console.WriteLine("Start: RWR...");

            List<int> result = new List<int>();

            if (T <= 0.0d)
                for (int i = 1; i <= I; i++)
                {
                    // Console.Write("Iteration {0}: ", i);
                    Compute();
                    //Console.Write(stderr);
                    //Console.WriteLine();
                }
            else
                while (stderr > T)
                    stderr = Compute();

            /*
            Console.WriteLine("print case!!");
            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine(cs[i]);
            }
            */

            for (int i = 0; i < score.Length; i++)
                if(score[i]>0)
                    result.Add(i);
            //result.Sort(Weight.CompareWeightDesc);

            return result;
        }

        private double Compute()
        {
            double[] bal = graph.balance;
            int user_size = graph.user_size;

            double[] ptempscore = new double[user_size];
            double[] ntempscore = new double[user_size];

            for (int i = 0; i < user_size; i++)
            {
                SortedSet<Weight> links = graph.GetOutlinks(i);
                if (links == null) continue;

                int sum = links.Count();
                double wgt = (double)1 / (double)sum;


                double b0 = wgt * bal[0];
                double b1 = wgt * bal[1];
                double b2 = wgt * bal[2];
                double b3 = wgt * bal[3];


                double b0_conjugate = wgt - b0;
                double b1_conjugate = wgt - b1;
                double b2_conjugate = wgt - b2;
                double b3_conjugate = wgt - b3;

                foreach (Weight j in links)
                {
                    if(j.id < user_size)
                    {
                        if (j.w > 0)
                        {
                            ptempscore[j.id] += pscore[i] * b0 + nscore[i] * b2_conjugate;
                            ntempscore[j.id] += pscore[i] * b0_conjugate + nscore[i] * b2;
                        }
                        else
                        {
                            ptempscore[j.id] += pscore[i] * b1_conjugate + nscore[i] * b3;
                            ntempscore[j.id] += pscore[i] * b1 + nscore[i] * b3_conjugate;
                        }
                    }
               
                }
            }
            
            for(int i = 0; i < user_size; i++)
            {
                pscore[i] = ptempscore[i] * D;
                nscore[i] = ntempscore[i] * D;
            }
            pscore[init_node] += 1.0d - D;

            double stderr = 0, tmpscore;

            for (int i = 0; i < score.Length; i++)
            {
                tmpscore = pscore[i] - nscore[i];
                stderr += Math.Pow(score[i] - tmpscore, 2);
                score[i] = tmpscore;
            }
            return stderr;
        }

        public void PrintScore()
        {
            for (int i = 0; i < score.Length; i++)
                //Console.WriteLine(i.ToString() + "\t" + score[i].ToString());

                return;
        }
    }
}


/*
                    // give score
                    foreach (Weight j in links)
                    {
                        if (i < user_size)
                        {
                            if (j.id < user_size)
                            {
                                if (j.w > 0)
                                {
                                    cs[0]++;
                                    ptempscore[j.id] += (double)(pscore[i] * (j.w / sum));
                                    ptempscore[j.id] += (1 - gamma) * (double)(nscore[i] * (j.w / sum));
                                    ntempscore[j.id] += (gamma) * (double)(nscore[i] * (j.w / sum));
                                }
                                else
                                {
                                    cs[1]++;
                                    ptempscore[j.id] += (beta) * (double)(nscore[i] * (-j.w / sum));
                                    ntempscore[j.id] += (double)(pscore[i] * (-j.w / sum));
                                    ntempscore[j.id] += (1 - beta) * (double)(nscore[i] * (-j.w / sum));
                                }
                            }
                            else
                            {
                                if (j.w > 0)
                                {
                                    cs[2]++;
                                    ptempscore[j.id] += (double)(pscore[i] * (j.w / sum));
                                    ptempscore[j.id] += (1 - delta) * (double)(nscore[i] * (j.w / sum));
                                    ntempscore[j.id] += (delta) * (double)(nscore[i] * (j.w / sum));
                                }
                                else
                                {
                                    cs[3]++;
                                    ntempscore[j.id] += (double)(pscore[i] * (-j.w / sum));
                                    ptempscore[j.id] += (delta) * (double)(nscore[i] * (-j.w / sum));
                                    ntempscore[j.id] += (1 - delta) * (double)(nscore[i] * (-j.w / sum));
                                }
                            }
                        }
                        else
                        {
                            if (j.id < user_size)
                            {
                                if (j.w > 0)
                                {
                                    cs[4]++;
                                    ptempscore[j.id] += (double)(pscore[i] * (j.w / sum));
                                    ptempscore[j.id] += (1 - delta) * (double)(nscore[i] * (j.w / sum));
                                    ntempscore[j.id] += (delta) * (double)(nscore[i] * (j.w / sum));
                                }
                                else
                                {
                                    cs[5]++;
                                    ptempscore[j.id] += (double)(nscore[i] * (-j.w / sum));
                                    ptempscore[j.id] += (1 - delta) * (double)(pscore[i] * (-j.w / sum));
                                    ntempscore[j.id] += (delta) * (double)(pscore[i] * (-j.w / sum));
                                }
                            }
                            else
                            {
                                if (j.w > 0)
                                {
                                    cs[6]++;
                                    ptempscore[j.id] += (double)(pscore[i] * (j.w / sum));
                                    ptempscore[j.id] += (1 - delta) * (double)(nscore[i] * (j.w / sum));
                                    ntempscore[j.id] += (delta) * (double)(nscore[i] * (j.w / sum));
                                }
                                else
                                {
                                    cs[7]++;
                                    ntempscore[j.id] += (double)(pscore[i] * (-j.w / sum));
                                    ptempscore[j.id] += (delta) * (double)(nscore[i] * (-j.w / sum));
                                    ntempscore[j.id] += (1 - delta) * (double)(nscore[i] * (-j.w / sum));
                                }
                            }
                        }
                    }

                SparseCompressedColumnMatrix<double> testM;
            testM = Matrix.CreateSparse<double>(3, 3);
            testM[0, 0] = 0.1d;
            testM[0, 2] = 0.2d;
            testM[2, 0] = 0.3d;
            testM[1, 2] = 0.4d;

            DenseVector<double> testV = Vector.Create<double>(3);
            testV[0] = 0.3d;
            testV[1] = 0.2d;
            testV[2] = 0.1d;

            DenseVector<double> resultV = Vector.Create<double>(3);
            resultV = Matrix.Multiply<double>(testM, TransposeOperation.Transpose, testV).ToDenseVector();
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("{0}", resultV[i]);
            }
    */
