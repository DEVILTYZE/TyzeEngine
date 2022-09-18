#version 330 core
#define NR_LIGHTS 32

in vec2 textureCoordinates;
in vec3 normal;
in vec3 fragmentPosition;

out vec4 outputColor;

struct Material
{
	sampler2D diffuse;
	sampler2D specular;
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

uniform sampler2D textureColor;
uniform vec4 inColor;
uniform int pointLightCount;
uniform int spotLightCount;
uniform DirectionLight dirLight;
uniform PointLight pointLight[NR_LIGHTS];
uniform SpotLight spotLight[NR_LIGHTS];
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
		diffuseColor = vec3(texture(material.diffuse, textureCoordinates));
		specularColor = vec3(texture(material.specular, textureCoordinates));
	}
	
	vec3 viewDirection = normalize(-fragmentPosition);
	vec3 resultColor = CalcDirectionLight(dirLight, normal, viewDirection, diffuseColor, specularColor);
	
	for (int i = 0; i < pointLightCount; ++i)
		resultColor += CalcPointLight(pointLight[i], normal, fragmentPosition, viewDirection, diffuseColor, specularColor);

	for (int i = 0; i < spotLightCount; ++i)
		resultColor += CalcSpotLight(spotLight[i], normal, fragmentPosition, viewDirection, diffuseColor, specularColor);
	
	outputColor = vec4(resultColor, 1.0);
}
