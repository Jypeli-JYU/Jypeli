#version 300 es
layout (location = 0) in mediump vec4 vPos;
layout (location = 1) in mediump vec4 vCol;
layout (location = 2) in mediump vec2 vTex;

uniform mediump mat4 world;

out mediump vec4 fCol;
out mediump vec2 fTex;
out mediump vec2 texCoords;
out mediump vec4 vertpos;

void main()
{
    gl_Position =  world * vPos;
    fCol = vCol;
    texCoords = vTex;
    vertpos = gl_Position;
}