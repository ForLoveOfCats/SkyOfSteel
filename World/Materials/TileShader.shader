shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,skip_vertex_transform;


uniform sampler2D texture_albedo : hint_albedo;

// Using the skip_vertex_transform render mode means we have to explicitly apply model-space to view-space
// transforms. This allows for calculating world-space normals instead of view-space.
void vertex() {
	VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	NORMAL = (WORLD_MATRIX * vec4(NORMAL, 0.0)).xyz;
}

void fragment() {
	// Calculating how exposed the surface is to the sun (the hardcoded vec3 is the normalized direction to the sun)
	float dot_to_sun = dot(NORMAL, vec3(0.00299977, 0.99992351, 0.01199908));
	// Amount of light for the diffuse part of the lighting model
	float diffuse = (dot_to_sun + 1.0) / 4.3;
	// Amount of light for the ambient part of the lighting model
	float ambient = 0.655;
	
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	vec3 color = albedo_tex.rgb;
	
	ALBEDO = (ambient + diffuse) * color;
}
