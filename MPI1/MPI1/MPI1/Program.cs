using System;
using MPI;


namespace MPI1
{
    class Program
    {

        public static int procid, numproc, start, finish, j;

        const int n = 10;
        static void Main(string[] args)
        {

            using (new MPI.Environment(ref args))
            {

                procid = Communicator.world.Rank;
                numproc = Communicator.world.Size;
                Communicator.world.Barrier();

                start = 2 + procid * (n - 1) / numproc;
                finish = 1 + (procid + 1) * (n - 1) / numproc;

                //check every number in the range of this processor
                for (j = start; j <= finish; j++)
                {
                    //if the number is not composite, is prime, increment the local counter
                    if (checkPrime(j))
                    {
                        Console.WriteLine("prime: " + j);
                    }
                }


                Console.WriteLine("Process " + procid + " finished\n");
                //Communicator.world.Reduce<int>(start, Operation<int>.Add, 0);
            }

        }



        public static bool checkPrime(int number)
        {

            if (number == 1) return false;
            if (number == 2) return true;

            if (number % 2 == 0) return false;

            for (int i = 2; i < number; i++)
            {
                if (number % i == 0) return false;
            }

            return true;

        }




    }
}
