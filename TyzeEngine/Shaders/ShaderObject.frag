#version 330 core

in vec2 textureCoordinates;
in vec3 normal;
in vec3 fragmentPosition;
in vec3 lightPos;

out vec4 outputColor;

struct Light
{
	float ambient;
	float specular;
	float shininess;
};

uniform sampler2D texture0;
uniform vec4 lightColor;
uniform Light light;
uniform vec4 inColor;

void main() 
{
	// ambient
	vec4 ambient = lightColor * light.ambient;
	
	// diffuse
	vec3 lightDirection = normalize(lightPos - fragmentPosition);
	float diff = max(dot(normal, lightDirection), 0.0);
	vec4 diffuse = lightColor * diff;
	
	// specular
	vec3 viewDirection = normalize(-fragmentPosition);
	vec3 reflectDirection = reflect(-lightDirection, normal);
	float specularStrength = pow(max(dot(viewDirection, reflectDirection), 0.0), light.shininess);
	vec4 specular = light.specular * lightColor * specularStrength;
	
	if (inColor.w == 0)
		outputColor = (ambient + diffuse + specular) * texture(texture0, textureCoordinates);
	else
		outputColor = (ambient + diffuse + specular) * inColor;
}
