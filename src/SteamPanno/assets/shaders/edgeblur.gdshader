shader_type canvas_item;

uniform sampler2D src;
//uniform vec2 direction;
uniform int radiusMinX;
uniform int radiusMaxX;
uniform int radiusMinY;
uniform int radiusMaxY;

void fragment()
{
	vec2 pixel_size = 1.0 / vec2(textureSize(src, 0));
	vec4 color = texture(src, UV);
	
	float xMin = 0.0 + pixel_size.x / 2.0;
	float xMax = 1.0 - pixel_size.x / 2.0;
	float yMin = 0.0 + pixel_size.y / 2.0;
	float yMax = 1.0 - pixel_size.y / 2.0;
	
	float intensity = UV.x;
	int radiusX = int(round(mix(float(radiusMinX), float(radiusMaxX), intensity)));
	int radiusY = int(round(mix(float(radiusMinY), float(radiusMaxY), intensity)));
	
	for (int x = -radiusX; x <= radiusX; x++)
	{
		for (int y = -radiusY; y <= radiusY; y++)
		{
			if (x == 0 && y == 0) continue;
			vec2 offset = vec2(float(x), float(y)) * pixel_size;
			vec2 uvAndOffset = UV + offset;
			uvAndOffset.x = clamp(uvAndOffset.x, xMin, xMax);
			uvAndOffset.y = clamp(uvAndOffset.y, yMin, yMax);
			color += texture(src, uvAndOffset);
		}
	}
	
	COLOR = color / float((radiusX * 2 + 1) * (radiusY * 2 + 1));
}