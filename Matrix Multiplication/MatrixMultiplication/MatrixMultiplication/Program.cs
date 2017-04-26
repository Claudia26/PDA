using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace MM
{
    class Program
    {
        public static int processID, numberOfProcesses, size, j, k;
        public static int n = 4;
        public static int start, finish;
        public static int[,] a = { { 1, 0, 0, 1 },
                                   { 0, 1, 1, 0 },
                                   { 0, 1, 1, 0 },
                                   { 1, 0, 0, 1 }
                                 };
       
       
        public static int[,] b = { { 1, 0, 0, 0 },
                                   { 0, 1, 0, 0 },
                                   { 0, 0, 1, 0 },
                                   { 0, 0, 0, 1 }
                                  };
        public static int[,] c = { { 0, 0, 0, 0},
                                   { 0, 0, 0, 0},
                                   { 0, 0, 0, 0},
                                   { 0, 0, 0, 0}
                                  };
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;

                processID = Communicator.world.Rank;

                numberOfProcesses = Communicator.world.Size;

                comm.Barrier();
                matrixMultiply(a, b, c, comm);

            }
        }
        public static void PrintMatrix(int[,] c)
        {
            if (processID == 0)
            {
                for (int i = 0; i < n; ++i)
                {
                    for (j = 0; j < n; ++j)
                    {
                        
                        Console.WriteLine("Rank "+processID+" c[" + i + "][" + j + "] = " + c[i, j]);
                    }
                }
            }
        }
        public static void matrixMultiply(int[,] a, int[,] b, int[,] c, Intracommunicator comm)
        {
            int i;
            int[] d = new int[16];
            for ( i = 0; i < 16; i++)
            {
                d[i] = 0;
                Console.WriteLine(d[k]);

            }
            size = n / numberOfProcesses;
            start = processID * size;
            finish = (processID + 1) * size;
            
            
            Console.WriteLine("Proc with rank: "+processID+" is getting from: "+start+" to: "+ finish);
            
            for (i = start; i <= finish; ++i)
            {
                for (j = 0; j < n; ++j)
                {
                    for (k = 0; k < n; ++k)
                    {
                        c[i,j] += a[i,k] * b[k,j];
                        
                       
                    }
                }
                
            }

            comm.Allgather<int>(c[i, j], ref d);
            for (k = 0; k < n; k++)
            {
                Console.WriteLine("d[ "+ k +"] = "+d[k]);
            }
           
           
           

        }

    }
}