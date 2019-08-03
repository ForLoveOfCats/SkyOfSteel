shader_type canvas_item;
uniform float alpha = 0.5;


void fragment() {
	COLOR = texture(TEXTURE, UV);
	if (texture(TEXTURE, UV).a != 0.0) {
		COLOR.a = (COLOR.a + alpha) / 2.0;
	}
}
