#version 330 core
#define LIGHTS_CNT_MAX 32
#define LIGHTS_CNT 2

in vec2 textureCoordinates;
in vec3 normal;
in vec3 fragmentPosition;

out vec4 outputColor;

struct Material
{
	sampler2D diffuse0;
	sampler2D diffuse1;
	sampler2D diffuse2;
	sampler2D diffuse3;
	sampler2D specular0;
	sampler2D specular1;
	sampler2D specular2;
	sampler2D specular3;
	float shininess;
};

struct DirectionLight
{
	vec3 direction;
	
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight
{
	vec3 position;

	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct SpotLight
{
	vec3 position;
	vec3 direction;
	
	float cutOff;
	float outerCutOff;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

uniform vec3 viewPos;
uniform sampler2D textureColor;
uniform vec4 inColor;
uniform int dirCount;
uniform int pointCount;
uniform DirectionLight dirLight;
uniform PointLight[LIGHTS_CNT_MAX] pointLight;
uniform SpotLight[LIGHTS_CNT_MAX] spotLight;
uniform Material material;

vec3 CalcDirectionLight(DirectionLight light, vec3 normal, vec3 viewDirection, vec3 diffColor, vec3 specColor)
{
	vec3 lightDir = normalize(-light.direction);
	
	// diffuse
	float diff = max(dot(normal, lightDir), 0.0);
	
	// specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDirection, reflectDir), 0.0), material.shininess);
	
	// result
	vec3 ambient = light.ambient * vec3(diffColor);
	vec3 diffuse = light.diffuse * diff * vec3(diffColor);
	vec3 specular = light.specular * spec * vec3(specColor);
	
	return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragmentPosition, vec3 viewDirection, vec3 diffColor, vec3 specColor)
{
	vec3 lightDir = normalize(light.position - fragmentPosition);
	
	// diffuse
	float diff = max(dot(normal, lightDir), 0.0);
	
	// specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDirection, reflectDir), 0.0), material.shininess);
	
	// attenuation
	float distance = length(light.position - fragmentPosition);
	float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
	
	// result
	vec3 ambient = light.ambient * vec3(diffColor);
	vec3 diffuse = light.diffuse * diff * vec3(diffColor);
	vec3 specular = light.specular * spec * vec3(specColor);
	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;
	
	return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragmentPosition, vec3 viewDirection, vec3 diffColor, vec3 specColor)
{
	vec3 lightDir = normalize(light.position - fragmentPosition);
	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
	
	// diffuse
	float diff = max(dot(normal, lightDir), 0.0);

	// specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDirection, reflectDir), 0.0), material.shininess);
	
	// result
	vec3 ambient = light.ambient * vec3(diffColor);
	vec3 diffuse = light.diffuse * diff * vec3(diffColor);
	vec3 specular = light.specular * spec * vec3(specColor);
	diffuse *= intensity;
	specular *= intensity;
	
	return (ambient + diffuse + specular);
}

void main() 
{
	vec3 diffuseColor = vec3(inColor);
	vec3 specularColor = vec3(inColor);
	
	if (inColor.w == 0)
	{
		diffuseColor = vec3(texture(material.diffuse0, textureCoordinates));
		specularColor = vec3(texture(material.specular0, textureCoordinates));
	}
	
	vec3 viewDirection = normalize(viewPos - fragmentPosition);
	vec3 resultColor = CalcDirectionLight(dirLight, normal, viewDirection, diffuseColor, specularColor);
	
	for (int i = 0; i < dirCount; ++i)
		resultColor += CalcPointLight(pointLight[i], normal, fragmentPosition, viewDirection, diffuseColor, specularColor);

	for (int i = 0; i < pointCount; ++i)
		resultColor += CalcSpotLight(spotLight[i], normal, fragmentPosition, viewDirection, diffuseColor, specularColor);
	
	outputColor = vec4(resultColor, 1.0);
}
