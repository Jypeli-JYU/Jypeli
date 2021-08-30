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
    ivec2 texSize = textureSize(screenTexture, 0);

    float distance = distance(vec2(vertpos.x * texSize.x / 2, vertpos.y * texSize.y / 2), fPos); // TODO: Miksi kahdella jako?

    if (distance < fRadius)
    {
        alpha = mix(fIntensity, 0.0, distance/fRadius);
        FragColor = vec4(fCol.x, fCol.y, fCol.z, 1) * alpha;
    }else
    {
        FragColor = vec4(0, 0, 0, 0);
    }
}