#version 330 core

in vec2 texCoords;
in vec4 fCol;

uniform sampler2D tex;
out vec4 FragColor;

void main()
{
   FragColor = texture(tex, texCoords);
};