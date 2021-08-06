#version 330 core

in vec2 texCoords;
in vec4 fCol;
in float falpha;

uniform sampler2D tex;
out vec4 FragColor;

void main()
{
   FragColor = texture(tex, texCoords) * falpha;
};