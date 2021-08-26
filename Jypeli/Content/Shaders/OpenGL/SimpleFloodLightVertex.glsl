#version 330 core

layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec2 vTex;
layout (location = 3) in vec2 pos;
layout (location = 4) in float radius;
layout (location = 5) in float intensity;
layout (location = 6) in vec4 col;

uniform mat4 world;
uniform float scale;

out vec4 vertpos;
out vec4 fCol;
out vec2 fPos;
out float fIntensity;
out float fRadius;

void main()
{
    gl_Position = vec4(vPos.x, vPos.y, 0, 1);

    fCol = col;
    fPos = pos;
    fIntensity = intensity;
    fRadius = radius;
    vertpos = vPos;
};