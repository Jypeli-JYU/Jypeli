#version 300 es

in mediump vec2 texCoords;
in mediump vec4 fCol;
in mediump vec2 fTex;
in mediump vec4 vertpos;

out mediump vec4 FragColor;

uniform sampler2D screenTexture;


void main()
{
    FragColor = fCol;
}