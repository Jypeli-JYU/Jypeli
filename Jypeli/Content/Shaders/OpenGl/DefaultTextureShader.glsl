#version 330 core

in vec2 texCoords;
in vec4 fCol;

out vec4 FragColor;

uniform sampler2D tex;

void main()
{
    FragColor = texture(tex, texCoords) * fCol;
}