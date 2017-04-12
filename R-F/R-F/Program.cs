using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;
namespace R_F
{
    class Program
    {
        public const int INF = 99999;
        public static int[,] graph = {
            { 0,   2,  INF, 10 },
            { INF, 0,   1, INF },
            { INF, INF, 0,   1 },
            { INF, INF, INF, 0 }
        };
        static int processID, numberOfProcesses, n = 4, j, k, constant=4;

        static void Main(string[] args)
        {

            k = 0;
            using (new MPI.Environment(ref args))
            {
                
                Intracommunicator comm = Communicator.world;
                
                processID = Communicator.world.Rank;
               
                numberOfProcesses = Communicator.world.Size;
                
                FloydWarshall(graph, constant, processID, numberOfProcesses, comm);
             }
            
        }
       

        private static void Print(int[,] distance, int verticesCount, int processID)
        {

            for (int i = 0; i < verticesCount; ++i)
            {
                for (int j = 0; j < verticesCount; ++j)
                {
                    if (processID == 0)
                    {

                        if (distance[i, j] == INF)
                            Console.Write("INF".PadLeft(7));
                        else
                            Console.Write(distance[i, j].ToString().PadLeft(7));
                    }
                }

            }
        }

        public static void FloydWarshall(int[,] graph, int verticesCount, int processID, int numberOfProcesses, Intracommunicator comm)
        {
            int[,] distance = new int[verticesCount, verticesCount];

           
            int size = constant/numberOfProcesses;
            int start = processID * size;
            int finish = (processID + 1) * size;
            for (k = 0; k < verticesCount; k++)
            {
              
                    for (int i = start; i < finish; ++i)
                        for (int j = 0; j < verticesCount; ++j)
                        {
                             distance[i, j] = graph[i, j];
                            
                        }
                
            }

            for (k = 0; k < verticesCount; k++)
            {
                for (int i = start; i < finish; ++i)
                {
                    for (int j = 0; j < verticesCount; ++j)
                    {
                        if (distance[i, k] + distance[k, j] < distance[i, j])
                            distance[i, j] = distance[i, k] + distance[k, j];
                    }
                }
            }
            
            comm.Allgather<int[,]>(distance, 0);
           Print(distance, verticesCount, processID);
           
        }
    }
}
