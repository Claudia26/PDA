using System;
using MPI; 


namespace MPI1
{
    class Program
    {
		public static int i,j;
		public static int start,finish;
		public static int processId, nrPerProcessor;
		
        const int n = 10;
		
        public static bool isPrime(int number)
        {

            if (number == 1) return false;
            if (number == 2) return true;
            
            // Math.Ceiling - Returns the smallest integral value that is greater than or equal to the specified double-precision floating-point number.

            for (int i = 2; i <= Math.Ceiling(Math.Sqrt(number)); ++i)
            {
                if (number % i == 0) return false;
            }

            return true;

        }

        
           

        static void Main(string[] args)
        {
            int primeNr = 0;
			int totalNr = 0;
			
            //initialize MPI Environment
            using (new MPI.Environment(ref args))
            {
                // each communicator has an unique idenification id in order to specify the source and the destination
                processId = Communicator.world.Rank;

                // size of communicator
                nrPerProcessor = Communicator.world.Size;
                
                //Blocks the current process until all other processes in the current communicator have reached this routine.
                Communicator.world.Barrier();

                start = 2 + processId * (n - 1) / nrPerProcessor;
                finish = 1 + (processId + 1) * (n - 1) / nrPerProcessor;

                Intracommunicator world = Communicator.world;
                              
               
                for (j = start; j <= finish; j++)
                { 
                        if (isPrime(j))
                        {
                            ++primeNr;
                            Console.WriteLine("The prime nr {0} has been found", j);
                        }
                }

                if (world.Rank == 0)
                {
                    //world.Reduce - Combines the values sent by all processes using a predefined operator and places result in the receive buffer of the root process
                     totalNr = world.Reduce<int>(primeNr, Operation<int>.Add, 0);

                    System.Console.WriteLine("The Number of Prime Numbers found is: {0}", totalNr);
                }
                else
                {
                    world.Reduce<int>(primeNr, Operation<int>.Add, 0);
                }
            }

            
        }
    }
}
