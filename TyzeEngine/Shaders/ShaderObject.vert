#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 inNormal;
layout(location = 2) in vec2 inTexture;

out vec2 textureCoordinates;
out vec3 normal;
out vec3 fragmentPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat3 normalMatrix;

void main() 
{
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
	fragmentPosition = vec3(view * model * vec4(aPosition, 1.0));
	normal = normalize(normalMatrix * inNormal);
    textureCoordinates = inTexture;
}
