using System;
using System.Collections;
using System.Threading;

using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct elementQueue
{
    public int start;
    public int end;
    public int threadNumber;
    public int taskType;        // 1 - for computePath(), 2 - for write path (only once used -> the first time);
    public String cameFrom;     //0 from the first divide et impera call, 1 from the second
};

struct matrixLimits
{
    public int begin;
    public int end;
    public int threadNumber;
    // public int threadStatus = 0; //;0 - nu a pornit; 1 - in executie; 2 - a incheiat executia  
};

struct imperaObject
{
    public String cameFrom;
    public int value;
};

public class Test
{
    int x = 1, y = 3;
    //static ProducerConsumer prodCons;
    volatile bool verifyEnd = false, finishThreads = false, firstTaskAdded = false;
    static int m, n, INFINIT = 90000;
    int[][] cost;

    Thread verificator;
    readonly object full = new object();
    readonly object empty = new object();

    readonly object startPartTwo = new object();
    static readonly object finish = new object();

    readonly object active = new object();
    int numberOfVerifications = 0;

    int readyToGo = 0;
    static int comparari = 0;
    int mod = 0;

    static int numberOfThreadsReady = 0;
    static Mutex notr = new Mutex();

    Mutex mutex = new Mutex();
    Mutex mutex2 = new Mutex();
    Mutex mutex3 = new Mutex();
    Mutex mutex4 = new Mutex();
    Mutex mutex5 = new Mutex();
    Mutex ver = new Mutex();
    Queue queue = new Queue();
    static int NR_MAXIM = 6;
    static int[] busyThreads = new int[NR_MAXIM];
    Thread[] firstThreads = new Thread[NR_MAXIM];

    Mutex[] busyThreadsMutexes = new Mutex[NR_MAXIM];

    Mutex mutexImpera = new Mutex();
    Mutex mutexVerify = new Mutex();
    List<imperaObject> imperaList = new List<imperaObject>();


    static void Main()
    {

        //aici era un while pentru introducerea numerelor
        Test p = new Test();
        p.initCosts();

        long millisecondsStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        p.firstPart();
        
        p.secondPart();
        p.waitToEnd();
        long millisecondsEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        Console.WriteLine("Milisecunde: " + (millisecondsEnd - millisecondsStart));
        p.showResult();

       
    }

    public void showResult()
    {
        imperaObject aux = new imperaObject();
        for (int i = 0; i < imperaList.Count - 1; i++)
            for (int j = i + 1; j < imperaList.Count; j++)
                if (imperaList[i].cameFrom.CompareTo(imperaList[j].cameFrom) > 0)
                {
                    aux = imperaList[i];
                    imperaList[i] = imperaList[j];
                    imperaList[j] = aux;
                }
        Console.WriteLine("Final results:------------------");
        Console.Write(x + " ");
        for (int i = 0; i < imperaList.Count; i++)
            Console.Write(imperaList[i].value + "  ");
        Environment.Exit(0);
    }
    public void waitToEnd()
    {
        lock (finish)
        {
            Monitor.Wait(finish);
        }

        //  for (int i = 0; i < NR_MAXIM; i++)
        //    firstThreads[i].Abort();

        Console.WriteLine("Gataaa!");
        //for (int index = 0; index < NR_MAXIM; index++)
        //{
        //    firstThreads[index].Interrupt();// = null;//new ParameterizedThreadStart(Consume));
        //    //firstThreads[index].IsBackground = true;
        //    //firstThreads[index].Start(index);
        //}
        Console.WriteLine("Ar trebui sa se termine acum!");
        
        //Thread.Sleep(1000);
    }

    public void firstPart()
    {
        int divisations = n / NR_MAXIM;

        Console.WriteLine("n=" + n + " NR_MAXIM=" + NR_MAXIM + " divisations = " + divisations);

        int i = 0;
        for (i = 0; i < NR_MAXIM - 1; i++)
        {
            matrixLimits ml = new matrixLimits();
            ml.begin = i * divisations + 1;
            ml.end = (i + 1) * divisations;
            ml.threadNumber = i + 1;
           // Console.WriteLine("thread-ul " + ml.threadNumber + " (" + ml.begin + "," + ml.end + ")");

            firstThreads[i] = new Thread(new ParameterizedThreadStart(RoyFloyd));
            firstThreads[i].Start(ml);
        }

        matrixLimits lastMl = new matrixLimits();
        lastMl.begin = i * divisations + 1;
        lastMl.end = n;
        lastMl.threadNumber = i + 1;

        firstThreads[i] = new Thread(new ParameterizedThreadStart(RoyFloyd));
        firstThreads[i].Start(lastMl);

        // p.RoyFloyd();
        /*  
        for (int k = 0; k <= i; k++)
              firstThreads[k].Join();
        */
        int currentValue;
        notr.WaitOne();
        currentValue = numberOfThreadsReady;
        notr.ReleaseMutex();

        while (currentValue < i)
        {
            notr.WaitOne();
            currentValue = numberOfThreadsReady;
            notr.ReleaseMutex();
        }

        //displayMatrix();
        Console.WriteLine("Comparari: " + comparari);
    }
    public void secondPart()
    {

        Console.WriteLine("Incepe partea a 2-a ");
        Thread.Sleep(1000);
        // prodCons = new ProducerConsumer();

        elementQueue firstTask = new elementQueue();
        firstTask.start = x;
        firstTask.end = 3;
        firstTask.threadNumber = 1;
        firstTask.taskType = 2;

        for (int i = 0; i < NR_MAXIM; i++)
            busyThreads[i] = 1;

        //start all the threads as consumers
        //for (int index = 0; index < NR_MAXIM; index++)
        //{
        //    firstThreads[index] = new Thread(new ParameterizedThreadStart(Consume));
        //    firstThreads[index].IsBackground = true;
        //    firstThreads[index].Start(index);
        //}
        // Test test = new Test();
        Console.WriteLine("->push: Main Thread " + "added " + "writePath(" + x + "," + y + ")");

        Produce(firstTask);
        for(int i = 0; i <= NR_MAXIM; i++)
        lock (startPartTwo)
        {
            Monitor.Pulse(startPartTwo);
        }
    }

    public void initCosts()
    {

        int fileNumbers = 0;
        int[] numbers = ParseNumberFile("input.txt", ref fileNumbers);
        cost = new int[1000][];
        //for (int i = 0; i < fileNumbers; i++)
          //  Console.Write(numbers[i] + " ");

        n = numbers[0];
        m = numbers[1];

        for (int i = 1; i <= n; i++)
        {
            cost[i] = new int[1000];
            for (int j = 1; j <= n; j++)

                if (i == j)
                    cost[i][j] = 0;
                else
                    cost[i][j] = INFINIT;
        }
        int nr = 1;
        for (int i = 2; i < fileNumbers; i = i + 3)
        {
            int x = numbers[i];
            int y = numbers[i + 1];
            int c = numbers[i + 2];

            //Console.WriteLine("cost[" + x + "," + y + "]=" + c);
            cost[x][y] = c;

        }
        Console.WriteLine();
        /*for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= n; j++)
                if (cost[i][j] != INFINIT)
                    Console.Write(cost[i][j] + " ");
                else
                    Console.Write("- ");

            Console.WriteLine();
        }*/

        for (int z = 0; z < NR_MAXIM; z++)
            busyThreadsMutexes[z] = new Mutex();
    }

    void displayMatrix()
    {

        Console.WriteLine();
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= n; j++)
                if (cost[i][j] != INFINIT)
                    Console.Write(cost[i][j] + " ");
                else
                    Console.Write("- ");

            Console.WriteLine();
        }
    }
    static int[] ParseNumberFile(string filename, ref int n)
    {
        string fileContent = File.ReadAllText(filename);
        string[] integerStrings = fileContent.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        int[] integers = new int[integerStrings.Length];

        for (n = 0; n < integerStrings.Length; n++)
            integers[n] = int.Parse(integerStrings[n]);

        return integers;
    }

    public void updateLimits(ref int start, ref int end)
    {
        //Console.Write("Update(" + start + "," + end + "):");

        int differenceEndStart = end - start;
        int differenceNEnd = n - end;
        int formula = 2 * (differenceEndStart + 1);

        if (end == n)
        {
            start = 1;
            end = n / NR_MAXIM;
            Console.WriteLine("end = n, start = 1, end = " + end);
        }
        else
            if (differenceNEnd >= formula)
            {
                start = end + 1;
                end = start + differenceEndStart;
                Console.WriteLine(differenceNEnd + " >= " + formula + "  start = " + start + " end " + end);

            }
            else
            {
                start = end + 1;
                end = n;
                Console.WriteLine("else" + "start " + start + " end " + end);

            }
    }
    void RoyFloyd(object obj)
    {
        matrixLimits ml = (matrixLimits)obj;

        Console.WriteLine("intra thread-ul " + ml.threadNumber + " (" + ml.begin + ", " + ml.end + ")");
        Thread.Sleep(1200);

        int currentStart = ml.begin;
        int currentEnd = ml.end;
        Boolean finished = false;

        while (finished == false)
        {
            for (int k = 1; k <= n && finished == false; k++)
            {
                for (int i = ml.begin; i <= ml.end && finished == false; i++)
                {
                    for (int j = currentStart; j <= currentEnd; j++)
                    {
                        mutex4.WaitOne();
                        //Console.WriteLine("(" + k + "," + i + "," + j + ")");
                        comparari++;
                        mutex4.ReleaseMutex();

                        if (cost[i][j] > cost[i][k] + cost[k][j])
                            cost[i][j] = cost[i][k] + cost[k][j];
                    }
                }
            }
            //the thread is ready to go forward. Increase the index.


            Console.WriteLine("dupa for thread-ul" + ml.threadNumber);
            mutex2.WaitOne();
            int currentReady = ++readyToGo;
            Console.WriteLine("Thread-ul " + ml.threadNumber + " face readyToGo " + currentReady + " maxim e " + NR_MAXIM);
            mutex2.ReleaseMutex();


            //wait untill all the threads are ready to go.
            while (currentReady < NR_MAXIM)
            {
                //Thread.Sleep(100);
                mutex2.WaitOne();
                currentReady = readyToGo;
                // Console.WriteLine(currentReady);
                mutex2.ReleaseMutex();
            }

            mutex3.WaitOne();
            mod++;
            if (mod % NR_MAXIM == 0 && mod > 0)
            {
                mutex2.WaitOne();
                readyToGo = 0;
                mutex2.ReleaseMutex();
            }
            mutex3.ReleaseMutex();

            //Thread.Sleep(1000);
            // mutex2.WaitOne();
            // readyToGo++;
            //mutex2.ReleaseMutex();

            // mutex2.WaitOne();
            // while (readyToGo < NR_MAXIM)
            // {
            //     mutex2.ReleaseMutex();
            //     Thread.Sleep(100);
            //     Console.WriteLine("Thread-ul " + ml.threadNumber + " face readyToGo " + readyToGo + " maxim e " + NR_MAXIM);
            //     mutex2.WaitOne();
            // }
            // mutex2.ReleaseMutex();
            //decrease readeyToGo variable for the next step
            //  (another method is to put only one of the threads to make it 0 but that also will require another variable and mutex)
            // mutex2.WaitOne();
            //readyToGo = 0;
            //mutex2.ReleaseMutex();

            Console.WriteLine("Thread-ul" + ml.threadNumber + " face readyToGo 0");

            // Thread.Sleep(1000);
            updateLimits(ref currentStart, ref currentEnd);

            Console.WriteLine("++++++++++++++Thread-ul " + ml.threadNumber + "(" + currentStart + "," + currentEnd + ")");
            if (currentStart == ml.begin && currentEnd == ml.end)
                finished = true;
        }

        notr.WaitOne();
        numberOfThreadsReady++;
        Console.WriteLine("Number of threads ready for the second part: " + numberOfThreadsReady);
        notr.ReleaseMutex();

        lock (startPartTwo)
        {
            Monitor.Wait(startPartTwo);
        }
        Console.WriteLine("Thread-ul " + ml.threadNumber + " trece la partea a 2-a");
        Consume(ml.threadNumber);
    }

    void computePath(Object arg)
    {
        int k = 1;
        bool ok = false;

        elementQueue eq = (elementQueue)arg;
        int i = eq.start;
        int j = eq.end;

        Console.WriteLine("<-pop Thread Number " + eq.threadNumber + " calls computePath(" + i + "," + j + ")");

        while (k <= n && !ok)
        {
            if (i != k && j != k)
                if (cost[i][j] == cost[i][k] + cost[k][j])
                {
                    elementQueue firstTask = new elementQueue();
                    firstTask.start = i;
                    firstTask.end = k;
                    firstTask.taskType = 1;
                    firstTask.cameFrom = eq.cameFrom + "0";

                    Produce(firstTask);

                    Console.WriteLine("->push: Thread Number " + eq.threadNumber + " added " + "computePath(" + i + "," + k + ")");

                    elementQueue secondTask = new elementQueue();
                    secondTask.start = k;
                    secondTask.end = j;
                    secondTask.taskType = 1;
                    secondTask.cameFrom += eq.cameFrom + "1";
                    Produce(secondTask);

                    Console.WriteLine("->push: Thread Number " + eq.threadNumber + " added " + "computePath(" + k + "," + j + ")");
                    //computePath(i, k);
                    //computePath(k, j);
                    ok = true;
                }
            k++;
        }
        if (!ok)
        {
            Console.WriteLine(j + " ");

            imperaObject iobj = new imperaObject();
            iobj.cameFrom = eq.cameFrom;
            iobj.value = j;

            mutexImpera.WaitOne();
            imperaList.Add(iobj);
            mutexImpera.ReleaseMutex();

        }

        //the thread becomes consumator
        Consume(eq.threadNumber);
    }

    void writePath(object obj/*int start, int stop*/)
    {
        int start, stop;
        elementQueue arg = (elementQueue)obj;

        start = arg.start;
        stop = arg.end;

        Console.WriteLine("<-pop Thread number " + arg.threadNumber + " calls function writePath(" + start + "," + stop + ") ");
        if (cost[start][stop] < INFINIT)
        {
            Console.WriteLine("path from " + start + " to " + stop + " has weight " + cost[start][stop]);
            Console.WriteLine(" The minimum cost path is: " + start);

            //change the type of task and add it in the queue.
            arg.taskType = 1;
            Produce(arg);

            //verificator = new Thread(new ParameterizedThreadStart(Verify));
            //verificator.Start(100);
            firstTaskAdded = true;

            Console.WriteLine("->push: Thread Number " + arg.threadNumber + " added " + "computePath(" + start + "," + stop + ")");
            //the thread becomes a consumer -> is waiting for a task.
            Consume(arg.threadNumber);


        }
        else
            Console.WriteLine("there is no path between " + start + " and " + stop);


    }

    // }
    //------------------------------------------------------------------------------


    //}

    //public class ProducerConsumer
    //{

    //one thread will verify if the others have completed their jobs,
    //only when one consumer tries to consume but the queue is empty
    //0 - busy thread(consumer or producer)
    //1 - idle thread(waiting to consume)

    public void Verify(object arg)
    {
        int indexThread = (int)arg;
        Console.WriteLine("Thread-ul " + indexThread + " este verificator");
        //Console.WriteLine("enter verify");
        int i;
        Boolean ready = false;

        while (!ready)
        {
            /*
                        ver.WaitOne();
                        while (true/*numberOfVerifications > 0)*/
            // {
            // numberOfVerifications--;
            //ver.ReleaseMutex();

            //lock (active)
            //{
            //    Monitor.Wait(active);
            //}

            Console.WriteLine("Verific!");

            for (i = 0; i < NR_MAXIM; i++)
            {
                this.busyThreadsMutexes[i].WaitOne();
                if (busyThreads[i] != 1)
                {
                    this.busyThreadsMutexes[i].ReleaseMutex();
                    break;
                }
                this.busyThreadsMutexes[i].ReleaseMutex();

            }

            if (i == NR_MAXIM)
            {
                lock (finish)
                {
                    Monitor.Pulse(finish);
                    // finishThreads = true;

                    //  break;
                }
                ready = true;
                Console.WriteLine("Toate threadurile asteapta, trebuie sa inceteze executia");
                //break;
            }
            Console.WriteLine(i + "/" + NR_MAXIM);
            //   ver.WaitOne();
            //}
            //ver.ReleaseMutex();
        }
    }

    public void Produce(object o)
    {
        //mutex.WaitOne();
        //int nr = queue.Count;
        //mutex.ReleaseMutex();

        //while (nr == NR_MAXIM - 1)
        //{
        //    lock (full)
        //    {
        //        Monitor.Wait(full);
        //    }
        //    mutex.WaitOne();
        //    nr = queue.Count;
        //    mutex.ReleaseMutex();
        //}

        mutex.WaitOne();
        queue.Enqueue(o);
        mutex.ReleaseMutex();

        lock (empty)
        {
            Monitor.Pulse(empty);
        }
    }

    public void Consume(object index)
    {
        //try
        //{
            int indexThread = (int)index;
            elementQueue popped;

            //mutex.WaitOne();
            Console.WriteLine("Consuming is : " + indexThread);
            // int nr = queue.Count;

            // mutex.ReleaseMutex();
            //lock (empty)
            //{
            mutex.WaitOne();
            while (!finishThreads &&  queue.Count == 0)
            {
                mutex.ReleaseMutex();

                Console.WriteLine("------------------The Queue is empty-----------------");

                busyThreadsMutexes[indexThread].WaitOne();
                busyThreads[indexThread] = 1;
                busyThreadsMutexes[indexThread].ReleaseMutex();

                if (firstTaskAdded)
                {
                    mutexVerify.WaitOne();
                    int i;
                    for (i = 0; i < NR_MAXIM; i++)
                    {
                        this.busyThreadsMutexes[i].WaitOne();
                        if (busyThreads[i] != 1)
                        {
                            this.busyThreadsMutexes[i].ReleaseMutex();
                            break;
                        }
                        this.busyThreadsMutexes[i].ReleaseMutex();

                    }

                    if (i == NR_MAXIM)
                    {
                        finishThreads = true;

                        lock (empty)
                        {
                           // for(int t = 0; t <= 100 * NR_MAXIM; t++)
                            Monitor.Pulse(empty);
                        }

                        lock (finish)
                        {
                            Monitor.Pulse(finish);
                        }
                        // ready = true;
                        Console.WriteLine("ELE: Toate threadurile asteapta, trebuie sa inceteze executia");
                    }

                    mutexVerify.ReleaseMutex();

                }
                if (!finishThreads)
                {
                    lock (empty)
                    {
                        Monitor.Wait(empty);
                    }
                }

                if (!finishThreads)
                    mutex.WaitOne();
                //else goto jump;
            }
        //if(mutex.
           // mutex.ReleaseMutex();

            if (!finishThreads)
            {
                //mutex.WaitOne();
                popped = (elementQueue)queue.Dequeue();
                mutex.ReleaseMutex();

                this.busyThreadsMutexes[indexThread].WaitOne();
                busyThreads[indexThread] = 0;
                this.busyThreadsMutexes[indexThread].ReleaseMutex();

                popped.threadNumber = indexThread;

                if (popped.taskType == 2)
                {
                    writePath(popped);
                }
                else
                {
                    computePath(popped);
                }


                //lock (full)
                //{
                //    Monitor.Pulse(full);
                //}
                // }
            }
        //jump: 
        Console.WriteLine("Thread " + indexThread + " ends gracefully");
       // }
        //catch (System.Threading.ThreadInterruptedException)
        //{
        //    Console.WriteLine("Thread " + index + " is dead");
        //}
        //}
    }
}