
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#define	INF 999
#include <stdio.h>


__global__ void floydKernel(int k, int **dev_graph)
{
	int i = threadIdx.x;
	int j = threadIdx.y;

	if (dev_graph[i, k] + (int)dev_graph[k, j] < dev_graph[i, j])
		dev_graph[i, j] = dev_graph[i, k] + (int)dev_graph[k, j];

}


cudaError_t floyd(int graph[5][5], unsigned int size, int distance[5][5])
{
	int **dev_graph = 0;
	cudaError_t cudaStatus;

	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_graph, size * sizeof(int*));
	for (int i = 0; i < size; i++)
	{
		dev_graph = cudaMalloc((void**)&dev_graph[i], size * sizeof(int));
		for (int j = 0; j < size; j++) {
			dev_graph[i][j] = graph[i][j];
		}

	}

	dim3 blockSize = dim3(size, size);
	int nrOfBlocks = 1;
	for (int k = 0; k < size; k++)
	{
		floydKernel << <nrOfBlocks, blockDim >> >(k, dev_graph);

	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
		goto Error;
	}

Error:
	cudaFree(dev_graph);

	return cudaStatus;
}



int main()
{
	int arraySize = 5;
	int graph[5][5] = {
		{ 0,   5,  INF, 10, 5 },
		{ INF, 0,   3, INF, 1 },
		{ INF, INF, 0,   1, 7 },
		{ INF, INF, INF, 0, 2 },
		{ 3, INF, 2, INF, 9 },
	};
	int distance[5][5];

	// Add vectors in parallel.
	cudaError_t cudaStatus = floyd(graph, arraySize, distance);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "addWithCuda failed!");
		return 1;
	}

	// cudaDeviceReset must be called before exiting in order for profiling and
	// tracing tools such as Nsight and Visual Profiler to show complete traces.
	cudaStatus = cudaDeviceReset();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceReset failed!");
		return 1;
	}

	return 0;
}

// Helper function for using CUDA to add vectors in parallel