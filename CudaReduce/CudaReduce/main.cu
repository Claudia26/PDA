#include <stdio.h>
#include <cstdlib>

// includes CUDA Runtime
#include <cuda_runtime.h>
#include <curand_kernel.h>
#include <cuda_runtime_api.h>
#include <cuda.h>

#define w 6
#define h 6
#define N w*h

__global__ void reduce(int *g_idata, int searchedNumber,  int *ok);
void fill_array(int *a, int n);

int main(void) {
	int a[N];
	int *dev_a;
	int size = N * sizeof(int); // we need space for 512 integers

								// allocate device copies of a, b, c
	cudaMalloc((void**)&dev_a, size);

	fill_array(a, N);
	cudaMemcpy(dev_a, a, size, cudaMemcpyHostToDevice);
	int searchedNumber;
	int ok = -1;

	printf("Type a number : ");
	scanf("%d", &searchedNumber);
	dim3 blocksize(6); // create 1D threadblock
	dim3 gridsize(N / blocksize.x);  //create 1D grid

	reduce << <gridsize, blocksize >> > (dev_a, searchedNumber, &ok);
	if (ok != -1) {
		("Found %d on %d position", searchedNumber, ok);
	}
	
	cudaFree(dev_a);
	scanf("%d");
	return 0;
}

__global__ void reduce(int *g_idata, int searchedNumber, int *ok) {

	int i = blockIdx.x * blockDim.x + threadIdx.x;
	//printf("%d ", i);

	__syncthreads();
	//printf("%d %d///", g_idata[i], searchedNumber);
	if (g_idata[i] == searchedNumber) {
		printf("Found %d on %d position %d", searchedNumber, i, *ok);
		*ok = i;
	}
}

void fill_array(int *a, int n)
{
	for (int i = 0; i < n; i++)
		a[i] = i;
}