using MPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchElement
{
    class UserOperations<T> : MPI.Operation<T>
    {
        public UserOperations(ReductionOperation<T> op) : base(op)
        {
        }

        public static int FindMaximumElement(List<int> numbers)
        {
            int maximum = int.MinValue;
            for (int i = 0; i <= numbers.Count; i++)
            {
                if (maximum < numbers[i])
                {
                    maximum = numbers[i];

                }
            }
            return maximum;
        }
    }
}
