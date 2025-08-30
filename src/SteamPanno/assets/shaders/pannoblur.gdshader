shader_type canvas_item;

uniform sampler2D src;

void fragment()
{
    vec2 pixel_size = 1.0 / vec2(textureSize(src, 0));
    vec4 color = texture(src, UV);
    
    for (int i = -2; i <= 2; i++) {
        for (int j = -2; j <= 2; j++) {
            if (i == 0 && j == 0) continue;
            vec2 offset = vec2(float(i), float(j)) * pixel_size;
            color += texture(src, UV + offset);
        }
    }
    
    COLOR = color / 25.0;
	//COLOR = vec4(1, 0, 0, 1);
}