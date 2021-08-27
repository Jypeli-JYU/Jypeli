#version 330 core

layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec2 vTex;
layout (location = 3) in vec2 pos;
layout (location = 4) in float rot;
layout (location = 5) in float pscale;
layout (location = 6) in vec4 col;

uniform mat4 world;
uniform float scale;

out vec4 fCol;
out vec2 texCoords;

void main()
{
    float x = (vPos.x * scale);
    float y = (vPos.y * scale);

    gl_Position =  world * vec4((x * cos(rot) - y * sin(rot) + pos.x / pscale) * pscale, (x * sin(rot) + y * cos(rot) + pos.y / pscale) * pscale, 0, 1);

    fCol = col;
    texCoords = vTex;
}