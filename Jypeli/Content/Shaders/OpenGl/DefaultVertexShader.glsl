#version 330 core
layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vUv;

uniform mat4 world;

out vec4 fCol;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position =  world * vPos;
    fCol = vUv;
}