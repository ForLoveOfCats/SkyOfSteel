shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx, skip_vertex_transform;


uniform sampler2D texture_albedo : hint_albedo;


const float min_light = 0.15f;
const float max_light = 0.82f;
const vec3 sun_normal = vec3(0, 1, 0); //Hardcoded perfectly vertical "sun", not updated to actual sun position


//Using the skip_vertex_transform render mode means we have to explicitly apply model-space to view-space
//transforms. This allows for calculating world-space normals instead of view-space.
void vertex() {
	VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	NORMAL = (WORLD_MATRIX * vec4(NORMAL, 0.0)).xyz;
}


void fragment() {
	//Calculating how exposed the surface is to the "sun"
	//Is active at all times including at night when there is no actual sun
	float dot_to_sun = dot(NORMAL, sun_normal); //Range of [-1, 1]
	float percent = (dot_to_sun + 1f) / 2f; //Range of [0, 1]
	float diffuse = ((max_light - min_light) * percent) + min_light; //Reslope value within bounds 

	vec3 color = texture(texture_albedo, UV).rgb;

	ALBEDO = diffuse * color;
}
