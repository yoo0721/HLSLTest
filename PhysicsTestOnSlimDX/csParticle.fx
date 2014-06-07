
struct Status
{
	float3 offset;
	float3 speed;
	float3 spinAxis;
	float1 spin;
};

RWStructuredBuffer<Status> statusBuffer: register(u2);
//StructuredBuffer<Status> statusBufferConst: register(u1);
[numthreads(100, 1, 1)]
void cs_main(uint id : SV_DispatchThreadID)
{
	uint i = id.x;

	//Status s = { float3(-10, -1, -1), float3(0, 0, 0), float3(0, 0, 1), 1 };
	
	
	
	statusBuffer[i].offset = statusBuffer[i].offset + statusBuffer[i].speed;
	//statusBuffer[i] = s;
}