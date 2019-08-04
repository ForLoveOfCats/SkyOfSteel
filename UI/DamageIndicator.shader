shader_type canvas_item;
uniform float alpha = 1;


void fragment()
{
	COLOR = texture(TEXTURE, UV);
	COLOR.a = COLOR.a * alpha;
}
