shader_type canvas_item;

uniform sampler2D src;
uniform vec2 direction;
uniform vec2 intensity_direction;
uniform int radiusMin;
uniform int radiusMax;

float gaussian(float radius, float n)
{
	float sigma = radius / 2.0;
	float squared_sigma = sigma * sigma;
	return float((1.0 / sqrt(2.0 * PI * squared_sigma)) * exp(-(n * n) / (2.0 * squared_sigma)));
}

float[16] init_weights(int radius)
{
	float[16] weights;
	if (radius < 1)
	{
		weights[0] = 1.0;
	}
	else
	{
		weights[0] = gaussian(float(radius), 0);
		float totalWeight = weights[0];
		for (int i = 1; i <= radius; i++)
		{
			weights[i] = gaussian(float(radius), float(i));
			totalWeight += weights[i] * 2.0;
		}
		for (int i = 0; i <= radius; i++)
		{
			weights[i] /= totalWeight;
		}
	}
	return weights;
}

void fragment()
{
	vec2 pixel_size = 1.0 / vec2(textureSize(src, 0));
	vec2 intensity = UV * intensity_direction + (1.0 - step(vec2(0,0), intensity_direction));
	int radius = int(round(mix(float(radiusMin), float(radiusMax), length(intensity))));
	float[16] weights = init_weights(radius);
	
	vec4 color = texture(src, UV) * weights[0];
	for(int i = 1; i <= radius; i++)
	{
		vec2 offset = direction * float(i) * pixel_size;
		color += texture(src, UV + offset) * weights[i];
		color += texture(src, UV - offset) * weights[i];
	}
	
	COLOR = color;
}