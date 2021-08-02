#version 330 core

in vec2 texCoords;
in vec4 fCol;
in vec2 fTex;

out vec4 FragColor;

uniform sampler2D screenTexture;
uniform int type;


void main()
{
    if(type == 0){ // color
        FragColor = fCol;
    } else if(type == 1){ // texture
        FragColor = texture(screenTexture, texCoords);
    }else if(type == 2){ // Colored texture
        FragColor = texture(screenTexture, texCoords) * fCol;
    }
}