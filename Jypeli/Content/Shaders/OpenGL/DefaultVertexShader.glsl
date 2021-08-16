#version 330 core
layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vCol;
layout (location = 2) in vec2 vTex;

uniform mat4 world;

out vec4 fCol;
out vec2 fTex;
out vec2 texCoords;
out vec4 vertpos;

void main()
{
    gl_Position =  world * vPos;
    fCol = vCol;
    texCoords = vTex;
    vertpos = gl_Position;
}