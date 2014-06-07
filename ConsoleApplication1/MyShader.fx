RWStructuredBuffer<int> MyBuffer :register (u0);
RWStructuredBuffer<int> MyBuffer2 :register (u1);


[numthreads(1, 1, 1)]
void MyComputeShader(uint3 threadID : SV_DispatchThreadID)
{
	//MyBuffer[threadID.x] *= 2;
	MyBuffer[threadID.x] = MyBuffer2[threadID.x];
}

technique
{
	pass P0
	{

	}
}