shader_type spatial;

uniform sampler2DArray textures;
uniform int step_time;
uniform int total_time;

void fragment() {
	int texture_number = (int(TIME * 1000.0) / step_time) % total_time;
	vec4 texture_color = texture(textures, vec3(UV, float(texture_number)));
	ALBEDO.rgb = texture_color.rgb;
}
