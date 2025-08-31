shader_type canvas_item;

uniform sampler2D src;
uniform int radius = 2;

void fragment()
{
	vec2 pixel_size = 1.0 / vec2(textureSize(src, 0));
	vec4 color = texture(src, UV);
	
	float xMin = 0.0 + pixel_size.x / 2.0;
	float xMax = 1.0 - pixel_size.x / 2.0;
	float yMin = 0.0 + pixel_size.y / 2.0;
	float yMax = 1.0 - pixel_size.y / 2.0;
	
	for (int i = -radius; i <= radius; i++)
	{
		for (int j = -radius; j <= radius; j++)
		{
			if (i == 0 && j == 0) continue;
			vec2 offset = vec2(float(i), float(j)) * pixel_size;
			vec2 uvAndOffset = UV + offset;
			uvAndOffset.x = clamp(uvAndOffset.x, xMin, xMax);
			uvAndOffset.y = clamp(uvAndOffset.y, yMin, yMax);
			color += texture(src, uvAndOffset);
		}
	}
	
	COLOR = color / pow(float(radius) * 2.0 + 1.0, 2);
}