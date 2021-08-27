#version 300 es

precision mediump float;

in vec2 texCoords;
in vec4 fCol;
in vec2 fTex;
in vec4 vertpos;

out vec4 FragColor;

uniform sampler2D screenTexture;

void main()
{
    FragColor = fCol;
}