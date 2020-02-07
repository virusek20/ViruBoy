#version 450 core

layout (location = 0) in vec4 position;
layout (location = 1) in vec2 uv;

out vec2 vs_uv;

layout(location = 20) uniform mat4 projection;


void main(void)
{
	gl_Position = projection * position;
	vs_uv = uv;
}