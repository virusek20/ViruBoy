#version 450 core

in vec2 vs_uv;

uniform sampler2D textureObject;

out vec4 color;

void main(void)
{
	color = texelFetch(textureObject, ivec2(vs_uv.x, vs_uv.y), 0);
}