#version 300 es

in mediump vec2 texCoords;
in mediump vec4 fCol;

out mediump vec4 FragColor;

uniform sampler2D tex;

void main()
{
    FragColor = texture(tex, texCoords) * fCol;
}