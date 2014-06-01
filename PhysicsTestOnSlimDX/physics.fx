//

matrix ViewProjection : register(b0);
static const uint vertexCount;
float sinf;
float cosf;
/*
VertexDefinition Textured_HW_VS(VertexDefinition input)
{
float s = sin(Angle);
float c = cos(Angle);
float x = input.Axis.x;
float y = input.Axis.y;
float z = input.Axis.z;

float4x4 lot =
{
x * x * (1 - c) + c,     x * y * (1 - c) - z *s,  z * x * (1 - c) + y * s, 0,
x * y * (1 - c) + z * s, y * y * (1 - c) + c,     y * z * (1 - c) - x * s, 0,
z * x * (1 - c) - y * s, y * z * (1 - c) + x * s, z * z * (1 - c) + c    , 0,
0                      , 0                      , 0                      , 0
};
}
*/

//tbuffer tbWorld { matrix matrixArray[10000]; };

struct VS_IN
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 texel : TEXCOORD;
	//row_major float4x4 world : MATRIX;
	matrix world : MATRIX;
	uint vertexID : SV_VertexID;
};

struct VS_OUT
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 texel : TEXCOORD0;
	float3 worldPos : POSITION;
};


Texture2D g_DecalMap : register(t0);
SamplerState g_Sampler : register(s0);


VS_OUT Textured_HW_Instancing_VS(VS_IN input)
{

	VS_OUT output;
	Matrix mat = transpose(input.world);
	output.pos = mul(input.pos, mat);
	output.worldPos = output.pos;
	output.pos = mul(output.pos, ViewProjection);
	output.color = input.color;
	output.texel = input.texel;

	//mat = matrixArray[0];
	//matrixArray[0] = mat;
	return output;
}


// PointLight
[maxvertexcount(3)]
void Textured_HW_Instancing_GS(triangle VS_OUT input[3], inout TriangleStream<VS_OUT> stream)
{
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
}

float4 Textured_HW_Instancing_PS(VS_OUT input) : SV_Target
{
	float4 output = g_DecalMap.Gather(g_Sampler, input.texel);
	float c = input.color.r;
	//float c = 0.5;
	output.r = output.r * c;
	output.g = output.g * c;
	output.b = output.b * c;
	return output;
}



technique11 Textured_HW_Instancing
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, Textured_HW_Instancing_VS()));
		SetGeometryShader(CompileShader(gs_5_0, Textured_HW_Instancing_GS()));
		SetPixelShader(CompileShader(ps_5_0, Textured_HW_Instancing_PS()));
	}
}











