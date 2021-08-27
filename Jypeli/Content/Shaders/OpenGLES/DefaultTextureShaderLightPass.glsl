#version 300 es

precision mediump float;

in vec2 texCoords;
in vec4 fCol;

out vec4 FragColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform vec4 ambientLight;

void main()
{
    FragColor = texture(texture0, texCoords) * (ambientLight + texture(texture1, texCoords));
}