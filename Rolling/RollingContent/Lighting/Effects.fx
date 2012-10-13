float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

float3 xLightDirection = float3(0,0,0); //directional
float xDirLightIntensity = 0.0f; //directional light intensity

float xAmbient = 0.5f; //ambient

float4 spec; //specular
float specularIntensity=1.0f; 
float4 specularColor;
float specularShininess=50000.0f;

float3 xPointLight1 = float3(0,0,0); //point light position
float xPointIntensity1 = 0.0f; //point light intensity
float3 xPointLight2 = float3(0,0,0); //point light position
float xPointIntensity2 = 0.0f; //point light intensity
float3 xPointLight3 = float3(0,0,0); //point light position
float xPointIntensity3 = 0.0f; //point light intensity

float3 xSpotPos = float3(0,0,0); //spot light position
float3 xSpotDir = float3(0, -1, 0);
float xSpotInnerCone = 0.3490;
float xSpotOuterCone = 0.6981;
float xSpotRange = 10.0f;
float xSpotIntensity = 1.0f;
bool xEnableLighting;

Texture xTexture;
sampler2D TextureSampler = sampler_state { 
	texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct TexVertexToPixel
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float LightingFactor: TEXCOORD0;
	float2 TextureCoords: TEXCOORD1;
	float3 Position3D : TEXCOORD2;
	float3 Normal : TEXCOORD3;
	float3 SpotLightDirection:TEXCOORD4;
};

struct TexPixelToFrame
{
	float4 Color : COLOR0;
};

TexVertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal:
NORMAL, float2 inTexCoords: TEXCOORD0)
{
	TexVertexToPixel Output = (TexVertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.TextureCoords = inTexCoords;
	Output.Position3D = mul(inPos, xWorld);
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	Output.LightingFactor = xDirLightIntensity;
	if (xEnableLighting)
	Output.LightingFactor *= saturate(dot(Normal, - xLightDirection));
	Output.Color=saturate(specularColor * specularIntensity);
	Output.SpotLightDirection = (xSpotPos - Output.Position3D) / xSpotRange;
	Output.Normal = normalize(mul(inNormal, (float3x3)xWorld));
	return Output;
}

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
	float3 lightDir = normalize(pos3D - lightPos);
	return dot(-lightDir, normal);
}

float GetSpotLightEffect(float3 lightDirection, float3 spotDirection)
{
	float2 CosineVector = cos(float2(xSpotOuterCone, xSpotInnerCone) * 0.5f);
	float LightDotSpot = dot(-lightDirection, normalize(spotDirection));
	return smoothstep(CosineVector.x, CosineVector.y, LightDotSpot);
}

float3 calcr(float3 direction, TexVertexToPixel PS)
{
    return normalize(2 * dot(xLightDirection, PS.Normal) * PS.Normal - xLightDirection);//fancy formula for calculating the reflected angle correctly
}

TexPixelToFrame TexturedPS(TexVertexToPixel PSIn)
{
	TexPixelToFrame Output = (TexPixelToFrame)0;
	
	float3 r = calcr(xLightDirection,PSIn); //direction of incoming light
	float3 temp = normalize(xView);
    float3 v = normalize(mul(normalize(xView), xWorld)); //normalize the view vector
	float4 spec = (specularIntensity) * specularColor; 

	//Finding the directional light's contribution to the intensity
	float dotProduct = dot(r, v); //take the dot product 
    float4 directionalSpecular = spec * max(pow(dotProduct, specularShininess), 0); //finding the specular light from the directional light
    float4 directional = directionalSpecular + PSIn.LightingFactor; //total directional light

	//Finding the first point light's contribution to the intensity
    float diffuseLightingFactor1 = DotProduct(xPointLight1,PSIn.Position3D, PSIn.Normal);
    diffuseLightingFactor1 = saturate(diffuseLightingFactor1)*xPointIntensity1;
    r = calcr(xPointLight1,PSIn);
    dotProduct = dot(r, v);
    float4 point1Specular = spec * max(pow(dotProduct, specularShininess), 0); //finding the specular light from the first point light
    float4 point1 = point1Specular + diffuseLightingFactor1;

	//Finding the second point light's contribution to the intensity
    float diffuseLightingFactor2 = DotProduct(xPointLight2,PSIn.Position3D, PSIn.Normal); 
    diffuseLightingFactor2 = saturate(diffuseLightingFactor2);
    diffuseLightingFactor2 *= xPointIntensity2;
    r = calcr(xPointLight2,PSIn);
    dotProduct = dot(r, v);
    float4 point2Specular = spec * max(pow(dotProduct, specularShininess), 0); //finding the specular light from the second point light
    float4 point2 = point2Specular + diffuseLightingFactor2;

	//Finding the third point light's contribution to the intensity
    float diffuseLightingFactor3 = DotProduct(xPointLight3, PSIn.Position3D, PSIn.Normal);
    diffuseLightingFactor3 = saturate(diffuseLightingFactor3);
    diffuseLightingFactor3 *= xPointIntensity3;
    r = calcr(xPointLight3,PSIn);
    dotProduct = dot(r, v);
    float4 point3Specular = spec * max(pow(dotProduct, specularShininess), 0); //finding the specular light from the third point light
    float4 point3 = point3Specular + diffuseLightingFactor3;

	float Attenuation = xSpotIntensity * saturate(1.0f -dot(PSIn.SpotLightDirection, PSIn.SpotLightDirection));
	//Attenuation *= GetSpotLightEffect(normalize(PSIn.SpotLightDirection),PSIn.SpotLightDirection);

	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
	Output.Color.rgb *= saturate(point1  + point2 + point3 + xAmbient + directional+Attenuation);
	return Output;
}

technique Textured
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 TexturedVS();
		PixelShader = compile ps_2_0 TexturedPS();
	}
}