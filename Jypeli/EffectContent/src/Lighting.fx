/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/*
 * A simple effect that provides lighting. There is a technique for both
 * textures and colors.
 *
 * The XNA BasicEffect class provides lighting as well. However, it
 * requires that normal vectors are included in the vertex data, which
 * would increase the data that is sent to the graphics card quite a lot.
 * This effect uses a normal vector that is constant.
 *
 * Author: Tero Jäntti.
 */

float4x4 xWorldViewProjection;
float4x4 xWorld;

// Point light.
float3 xLightPos;
float xLightPower;

// Light that illuminates all the objects evenly.
float xAmbient;

// Texture that is used with TextureLighting technique.
Texture xTexture;

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct TextureCoords
{
    float4 Position     : POSITION;    

    float2 TexCoords    : TEXCOORD0;
    float3 Position3D    : TEXCOORD2;
};

struct ColorPosition
{
    float4 Position     : POSITION;

    float4 Color        : COLOR0;
    float3 Position3D    : TEXCOORD2;
};

struct PixelToFrame
{
    float4 Color        : COLOR0;
};


TextureCoords TextureVertexShader(float4 inPos : POSITION0, float2 inTexCoords : TEXCOORD0)
{
    TextureCoords output = (TextureCoords)0;
    
    output.Position = mul(inPos, xWorldViewProjection);
    output.TexCoords = inTexCoords;
    output.Position3D = mul(inPos, xWorld);
    
    return output;
}

ColorPosition ColorVertexShader(float4 inPos : POSITION, float4 inColor : COLOR0)
{
    ColorPosition output = (ColorPosition)0;
    output.Position = mul(inPos, xWorldViewProjection);
    output.Color = inColor;
    output.Position3D = mul(inPos, xWorld);
    return output;
}


float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(-lightDir, normal);    
}


PixelToFrame IlluminateTexture(TextureCoords PSIn)
{
    PixelToFrame output = (PixelToFrame)0;        

    float3 normal = float3(0, 0, 1);

    float diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, normal);
    diffuseLightingFactor = saturate(diffuseLightingFactor);
    diffuseLightingFactor *= xLightPower;
    
    float4 baseColor = tex2D(TextureSampler, PSIn.TexCoords);
    
    output.Color.rgb = baseColor.rgb * (diffuseLightingFactor + xAmbient);
    output.Color.a = baseColor.a;

    return output;
}

PixelToFrame IlluminateColor(ColorPosition PSIn)
{
    PixelToFrame output = (PixelToFrame)0;
    float3 normal = float3(0, 0, 1);

    float diffuseLightingFactor = DotProduct(xLightPos, PSIn.Position3D, normal);
    diffuseLightingFactor = saturate(diffuseLightingFactor);
    diffuseLightingFactor *= xLightPower;
    
    float4 baseColor = PSIn.Color;
    
    output.Color.rgb = baseColor.rgb * (diffuseLightingFactor + xAmbient);
    output.Color.a = baseColor.a;
    return output;
}

technique TextureLighting
{
    pass Pass0
    {
         VertexShader = compile vs_2_0 TextureVertexShader();
         PixelShader = compile ps_2_0 IlluminateTexture();
    }
}

technique ColorLighting
{
    pass Pass0
    {
         VertexShader = compile vs_2_0 ColorVertexShader();
         PixelShader = compile ps_2_0 IlluminateColor();
    }
}
