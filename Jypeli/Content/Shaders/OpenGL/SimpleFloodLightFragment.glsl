#version 330 core

in vec4 fCol;
in vec2 fPos;
in float fIntensity;
in float fRadius;
in vec4 vertpos;


out vec4 FragColor;

uniform sampler2D screenTexture;

void main()
{
    float alpha = 1.0;

    float distance = distance(vec2(vertpos.x,vertpos.y), fPos);

    if (distance < fRadius)
    {
        alpha = mix(fIntensity, 0.0, distance/fRadius);
        FragColor += vec4(fCol.x, fCol.y, fCol.z, 1) * alpha;// + texture(screenTexture, fPos);
    }
}