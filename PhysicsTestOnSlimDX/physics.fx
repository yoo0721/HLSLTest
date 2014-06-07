//

matrix ViewProjection : register(b0);
float diffTime: register(b1);

struct Status
{
	float3 offset;
	float3 speed;
	float3 spinAxis;
	float1 spin;
};

Texture2D g_DecalMap : register(t0);
StructuredBuffer<Status> statusBufferConst: register(u1);

struct VS_IN
{
	float4 pos : SV_POSITION;
	float2 texel : TEXCOORD;
};


SamplerState g_Sampler : register(s0);


VS_IN Textured_HW_Instancing_VS(VS_IN input, uint instanceID : SV_InstanceID)
{
	VS_IN output;
	output.pos = input.pos + float4(statusBufferConst[instanceID].offset, 1);
	//float4x4 world = worldMatrixConst[instanceID];
		//world = transpose(world);
	//output.pos = mul(input.pos, world);
	//output.pos = output.pos + float4(1, 1, 1, 1);
	output.pos = mul(output.pos, ViewProjection);
	output.texel = input.texel;
	return output;
}


// PointLight
[maxvertexcount(3)]
void Textured_HW_Instancing_GS(triangle VS_IN input[3], inout TriangleStream<VS_IN> stream)
{
/*	
	VS_OUT temp;
	float3 v1 = input[1].worldPos - input[0].worldPos;
		float3 v2 = input[2].worldPos - input[0].worldPos;
		v1 = normalize(v1);
	v2 = normalize(v2);
	float3 n = cross(v1, v2);
		n = normalize(n);
	float3 v = input[0].worldPos + input[1].worldPos + input[2].worldPos;
		v = normalize(v);

	float c = dot(v, n);
	//if (c <= 0.2) c = 0.2f;
	//c = abs(c);
	c = sqrt(c);

	for (int i = 0; i < 3; i++)
	{
		temp = input[i];
		temp.color = float4(c, c, c, c);
		stream.Append(temp);
	}
	stream.RestartStrip();
	*/
	for (int i = 0; i < 3; i++)
	{
		stream.Append(input[i]);
	}
	stream.RestartStrip();
}

float4 Textured_HW_Instancing_PS(VS_IN input) : SV_Target
{
	float4 output = g_DecalMap.Gather(g_Sampler, input.texel);
	/*
	float c = input.color.r;
	//float c = 0.5;
	output.r = output.r * c;
	output.g = output.g * c;
	output.b = output.b * c;
	*/
	return output;
}




technique11 Textured_HW_Instancing
{
	pass P1
	{
		//SetComputeShader(CompileShader(cs_5_0, CS()));
		SetVertexShader(CompileShader(vs_5_0, Textured_HW_Instancing_VS()));
		SetGeometryShader(CompileShader(gs_5_0, Textured_HW_Instancing_GS()));
		SetPixelShader(CompileShader(ps_5_0, Textured_HW_Instancing_PS()));
	}


}











