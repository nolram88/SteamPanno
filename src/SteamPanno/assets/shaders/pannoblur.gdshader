shader_type canvas_item;

uniform float blur_amount : hint_range(0, 0.02) = 0.01;
uniform sampler2D src;

void fragment()
{
    vec2 pixel_size = 1.0 / vec2(textureSize(src, 0));
    vec4 color = texture(src, UV);
    
    for (int i = -2; i <= 2; i++) {
        for (int j = -2; j <= 2; j++) {
            if (i == 0 && j == 0) continue;
            vec2 offset = vec2(float(i), float(j)) * pixel_size * blur_amount;
            color += texture(src, UV + offset);
        }
    }
    
    COLOR = color / 25.0;
}