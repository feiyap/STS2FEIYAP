shader_type canvas_item;
render_mode blend_mix;

// ─── 全局控制 ─────────────────────────────────────────────────
uniform vec4  glow_color : source_color = vec4(0.95, 0.10, 0.10, 1.0);
uniform float slash_progress : hint_range(0.0, 1.0) = 0.0;
uniform int   slash_count = 0;

uniform vec2  slash_points_a[64];
uniform vec2  slash_points_b[64];
uniform float slash_weights[64];
uniform float slash_line_progress[64];

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

// ─── 红线本体 ─────────────────────────────────────────────────
uniform float core_width       : hint_range(0.0001, 0.005)  = 0.0005;
uniform float edge_width       : hint_range(0.00005, 0.003) = 0.00015;
uniform float core_brightness  : hint_range(0.0, 0.5)       = 0.05;

// ─── 红色辉光 ─────────────────────────────────────────────────
uniform float glow_width     : hint_range(0.0, 0.05) = 0.006;
uniform float glow_intensity : hint_range(0.0, 2.0)  = 0.40;

// ─── 切割错位
uniform float cut_strength    : hint_range(0.0, 0.05) = 0.028;
uniform float cut_inner       : hint_range(0.0, 0.02) = 0.003;
uniform float cut_outer_ratio : hint_range(0.0, 1.0)  = 0.20;
uniform float cut_far_ratio   : hint_range(0.0, 2.0)  = 0.40;

// ─── 双层阴影 ─────────────────────────────────────────────────
uniform float shadow_diffuse : hint_range(0.0, 1.0)   = 0.10;
uniform float shadow_edge    : hint_range(0.0, 1.0)   = 0.70;
uniform float edge_radius    : hint_range(0.0, 0.02)  = 0.008;


float dist_to_segment(vec2 p, vec2 a, vec2 b, out float t, out vec2 proj)
{
    vec2 ab = b - a;
    vec2 ap = p - a;
    t = clamp(dot(ap, ab) / max(dot(ab, ab), 1e-6), 0.0, 1.0);
    proj = a + t * ab;
    return distance(p, proj);
}

void get_visible_segment(int i, out vec2 va, out vec2 vb)
{
    vec2  a = slash_points_a[i];
    vec2  b = slash_points_b[i];
    float p = slash_line_progress[i];

    if (p <= 0.5) {
        float k = p / 0.5;
        va = a;
        vb = mix(a, b, k);
    } else {
        float k = (p - 0.5) / 0.5;
        va = mix(a, b, k);
        vb = b;
    }
}

void fragment()
{
    vec2 screen_uv = SCREEN_UV;
    vec2 uv = screen_uv;

    COLOR = vec4(0.0);
    if (slash_progress > 0.001) {

        float min_dist     = 1.0;
        float hit_alpha    = 0.0;
        float hit_end_fade = 1.0;
        float hit_line_len = 0.0;
        vec2  total_disp   = vec2(0.0);
        int   upper = min(slash_count, 64);

        for (int i = 0; i < 64; i++) {
            if (i >= upper) { break; }
            float w = slash_weights[i];
            if (w <= 0.001) { continue; }

            vec2 va, vb;
            get_visible_segment(i, va, vb);

            float t;
            vec2  proj;
            float d = dist_to_segment(uv, va, vb, t, proj);
            float end_fade_i = smoothstep(0.0, 0.06, t) * smoothstep(1.0, 0.94, t);

            float line_len = length(slash_points_b[i] - slash_points_a[i]);

            if (d < min_dist) {
                min_dist     = d;
                hit_alpha    = w;
                hit_end_fade = end_fade_i;
                hit_line_len = line_len;
            }

            float eff_outer = line_len * cut_outer_ratio;
            float eff_far   = line_len * cut_far_ratio;

            float ramp_up   = smoothstep(0.0, cut_inner, d);
            float ramp_down = 1.0 - smoothstep(eff_outer, eff_far, d);
            float field_i   = ramp_up * ramp_down * w * slash_progress * end_fade_i;

            if (field_i > 0.001) {
                vec2  ab     = vb - va;
                float ab_len = length(ab);
                vec2  line_dir    = (ab_len > 1e-5) ? ab / ab_len : vec2(1.0, 0.0);
                vec2  line_normal = vec2(-line_dir.y, line_dir.x);
                float side        = (dot(uv - proj, line_normal) >= 0.0) ? 1.0 : -1.0;

                total_disp += line_dir * (cut_strength * field_i * side);
            }
        }

        float disp_len = length(total_disp);
        if (disp_len > cut_strength) {
            total_disp *= cut_strength / disp_len;
        }

        float core_alpha = 1.0 - smoothstep(core_width, core_width + edge_width, min_dist);
        float glow_alpha = 1.0 - smoothstep(0.0, glow_width, min_dist);
        glow_alpha = pow(glow_alpha, 2.0);

        float life = hit_alpha * hit_end_fade * slash_progress;
        core_alpha *= life;
        glow_alpha *= life * glow_intensity;

        float eff_shadow_radius = hit_line_len * cut_outer_ratio;
        float shadow_field = (eff_shadow_radius > 1e-5)
            ? (1.0 - smoothstep(0.0, eff_shadow_radius, min_dist)) * life
            : 0.0;

        float edge_mask = 1.0 - smoothstep(0.0, edge_radius, min_dist);
        edge_mask = pow(edge_mask, 3.0) * life;

        float disp_alpha = clamp(disp_len / (cut_strength * 0.3), 0.0, 1.0);

        float any_alpha = max(max(max(core_alpha, glow_alpha), shadow_field),
                              max(edge_mask, disp_alpha));

        if (any_alpha > 0.001) {
            vec3 bg = texture(screen_texture, screen_uv + total_disp).rgb;

            bg *= (1.0 - shadow_field * shadow_diffuse);
            bg *= (1.0 - edge_mask * shadow_edge);

            vec3 with_glow  = bg + glow_color.rgb * glow_alpha;
            vec3 core_rgb   = mix(glow_color.rgb, vec3(1.0), core_brightness);
            vec3 final_rgb  = mix(with_glow, core_rgb, core_alpha);

            COLOR = vec4(final_rgb, clamp(any_alpha, 0.0, 1.0));
        }
    }
}
