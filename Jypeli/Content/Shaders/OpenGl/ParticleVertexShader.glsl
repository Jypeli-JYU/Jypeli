#version 330 core

layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec2 vTex;
layout (location = 3) in vec4 pos;

uniform mat4 world;
uniform float scale;

out vec4 fCol;
out vec2 texCoords;

void main()
{
    float x = (vPos.x * scale);
    float y = (vPos.y * scale);

    gl_Position =  world * vec4((x * cos(pos.z) - y * sin(pos.z) + pos.x / pos.w) * pos.w, (x * sin(pos.z) + y * cos(pos.z) + pos.y / pos.w) * pos.w, 0, 1);

    fCol = vCol;
    texCoords = vTex;
};