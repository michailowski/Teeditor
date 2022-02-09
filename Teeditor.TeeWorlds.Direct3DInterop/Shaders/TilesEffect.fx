Texture2DArray Textures : register(t0);
sampler TextureSampler : register(s0);


cbuffer Parameters : register(b0)
{
    row_major float4x4 MatrixTransform;
};


void TilesVertexShader(inout float4 color    : COLOR0,
                       inout float3 texCoord : TEXCOORD0,
                       inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);
}


float4 TilesPixelShader(float4 color    : COLOR0,
                        float3 texCoord : TEXCOORD0) : SV_Target0
{
    return Textures.Sample(TextureSampler, texCoord) * color;
}