shader_type spatial;
render_mode depth_draw_alpha_prepass, unshaded, cull_disabled;

uniform float alpha;


void fragment() {
	ALBEDO = vec3(1,1,1);
	ALPHA = alpha;
}
