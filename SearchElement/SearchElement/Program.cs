using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace SearchElement
{
    class Program
    {
        static int rank, size;
        static int nrToSearch = 18;
        static int nvalues;
        static int[] numbers = new int[50];
        static int[] positions = new int[50];
        static int i, j, temp;
        static bool inrange, found;
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
              
                Intracommunicator world = Communicator.world;
                rank = Communicator.world.Rank;
                size = Communicator.world.Size;
                Communicator.world.Barrier();
                found = false;

                if (rank == 0)
                {
                    for (i = 0; i < 50; ++i)
                    {
                        if (i % 10 == 0)
                        {
                            numbers[i] = 18;
                        }
                        else
                            numbers[i] = i;
                    }
                }

                Communicator.world.Broadcast<int[]>(ref numbers, 0);
                Communicator.world.ImmediateReceive<int>(rank, 1);

                nvalues = 50 / size;
                i = rank * nvalues;

                inrange = ((i <= ((rank + 1) * nvalues - 1)) & (i >= rank * nvalues));

                List<int> indexes = new List<int>();

                while (inrange)
                {
                    
                    if (numbers[i] == nrToSearch)
                    {
                        temp = 23;
                        indexes.Add(i);
                        for (j = 0; j < size; ++j)
                        {
                            Communicator.world.Send<int>(temp, j, 1);
                        }
                        Console.WriteLine("Process: " + rank + " has found number " + numbers[i] + " at global index " + i + "\n");
                        found = true;

                        
                      
                    }
                    ++i;
                    inrange = (i <= ((rank + 1) * nvalues - 1) && i >= rank * nvalues);
                }
                if (!found)
                {
                    Console.WriteLine("Process: " + rank + " stopped at global index " + (i - 1) + "\n");
                }

                int maximum = -1;
                for(i=0; i<indexes.Count; i++)
                {
                    if(maximum < indexes[i])
                    {
                        maximum = indexes[i];
                    }
                }

                    int high = world.Reduce(maximum, Operation<int>.Max, 0);
                    System.Console.WriteLine("The highest Index where element was found is " + high);
                
                }
               
            }

        }

    }

