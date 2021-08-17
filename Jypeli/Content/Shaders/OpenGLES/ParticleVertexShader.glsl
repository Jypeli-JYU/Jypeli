#version 300 es

layout (location = 0) in mediump vec4 vPos;
layout (location = 1) in mediump vec4 vCol;
layout (location = 2) in mediump vec2 vTex;
layout (location = 3) in mediump vec2 pos;
layout (location = 4) in mediump float rot;
layout (location = 5) in mediump float pscale;
layout (location = 6) in mediump vec4 col;

uniform mediump mat4 world;
uniform mediump float scale;

out mediump vec4 fCol;
out mediump vec2 texCoords;

void main()
{
    float x = (vPos.x * scale);
    float y = (vPos.y * scale);

    gl_Position =  world * vec4((x * cos(rot) - y * sin(rot) + pos.x / pscale) * pscale, (x * sin(rot) + y * cos(rot) + pos.y / pscale) * pscale, 0, 1);

    fCol = col;
    texCoords = vTex;
}