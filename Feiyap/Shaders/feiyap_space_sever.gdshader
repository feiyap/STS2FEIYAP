shader_type canvas_item;
render_mode unshaded;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;
uniform float elapsed = 0.0;
uniform vec2 cut_center = vec2(0.62, 0.48);

float hash(float x) { return fract(sin(x * 127.1) * 43758.5453); }
float noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float n = cell.x + cell.y * 71.0;
    return mix(mix(hash(n), hash(n + 1.0), f.x),
        mix(hash(n + 71.0), hash(n + 72.0), f.x), f.y);
}
float bell(float a, float b, float c, float x) {
    return smoothstep(a, b, x) * (1.0 - smoothstep(b, c, x));
}
void fragment() {
    float t = elapsed;
    float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
    vec2 metric = vec2(aspect, 1.0);
    vec2 p = (SCREEN_UV - cut_center) * metric;
    vec2 axis = normalize(vec2(1.0, 0.54));
    vec2 normal = vec2(-axis.y, axis.x);
    float along = dot(p, axis);
    float distance = dot(p, normal);
    float side = sign(distance);
    float enter = smoothstep(0.04, 0.24, t);
    float settle = 1.0 - smoothstep(0.70, 2.95, t);
    float split = enter * settle;
    vec2 displacement = side * (axis * 0.040 + normal * 0.012) * split;
    vec2 uv = SCREEN_UV - displacement / metric;
    float kick = bell(0.07, 0.20, 0.48, t);
    uv += vec2(sin(t * 35.0), cos(t * 29.0)) * kick * 0.0008;
    vec3 original = texture(screen_texture, clamp(uv, vec2(0.001), vec2(0.999))).rgb;
    float luminance = dot(original, vec3(0.2126, 0.7152, 0.0722));
    float mono = smoothstep(0.02, 0.30, t) * (1.0 - smoothstep(1.05, 1.95, t));
    vec3 color = mix(original, vec3(luminance), mono);
    color *= 1.0 - smoothstep(0.04, 0.32, t)
        * (1.0 - smoothstep(0.85, 1.65, t)) * 0.10;
    vec3 bladeColor = vec3(0.74, 0.78, 0.80);
    vec2 burstPoint = vec2(p.x * 1.15, p.y * 2.1);
    float angle = atan(burstPoint.y, burstPoint.x);
    float radius = length(burstPoint);
    float ragged = 0.23 + 0.15 * noise(vec2(angle * 9.0, 7.0))
        + 0.06 * noise(burstPoint * 24.0);
    float ink = (1.0 - smoothstep(ragged * 0.65, ragged, radius))
        * smoothstep(0.12, 0.60, noise(burstPoint * 18.0))
        * bell(0.23, 0.34, 0.78, t);
    color = mix(color, color * 0.35, ink * 0.55);
    float gap = (1.0 - smoothstep(0.001, 0.0065 * split + 0.0015, abs(distance))) * split;
    color *= 1.0 - gap * 0.96;
    float brush = bell(0.06, 0.20, 0.52, t);
    float edgeNoise = (hash(floor(along * 180.0)) - 0.5) * 0.010 * brush;
    float broadWidth = 0.041 * brush;
    float front = mix(-1.8, 1.6, smoothstep(0.055, 0.24, t));
    float sweep = 1.0 - smoothstep(front - 0.20, front, along);
    float broad = (1.0 - smoothstep(broadWidth * 0.70, broadWidth + 0.002,
        abs(distance + edgeNoise))) * brush * sweep;
    float seam = enter * (1.0 - smoothstep(2.65, 3.12, t));
    float thin = (1.0 - smoothstep(0.00065, 0.0018, abs(distance))) * seam;
    float glow = exp(-abs(distance) * 95.0) * (brush * 0.12 + seam * 0.035);
    color = mix(color, bladeColor, clamp(glow, 0.0, 1.0));
    color = mix(color, bladeColor, clamp(broad * 0.80 + thin * 0.65, 0.0, 0.85));
    float scratches = 0.0;
    for (int i = 0; i < 7; i++) {
        float f = float(i);
        float offset = (hash(f + 10.0) - 0.5) * 0.11;
        float taper = pow(max(0.0, 1.0 - abs(along + (hash(f) - 0.5))), 2.0);
        float line = abs(distance - offset - sin(along * 4.0 + f) * 0.01);
        scratches += (1.0 - smoothstep(0.0006, 0.0025, line)) * taper;
    }
    color = mix(color, bladeColor, clamp(scratches * brush * 0.24, 0.0, 0.45));
    vec2 crescentPoint = (p - vec2(0.06, -0.05)) / vec2(0.42, 0.13);
    float crescentRadius = length(crescentPoint);
    float crescentAngle = atan(crescentPoint.y, crescentPoint.x);
    float crescentWidth = 0.015 + 0.065 * smoothstep(-0.8, 2.5, crescentAngle);
    float crescent = (1.0 - smoothstep(crescentWidth, crescentWidth + 0.025,
        abs(crescentRadius - 1.0))) * smoothstep(-2.9, -2.0, crescentAngle);
    color = mix(color, bladeColor, clamp(crescent * bell(0.12, 0.24, 0.48, t) * 0.45, 0.0, 0.5));
    float frame = smoothstep(0.06, 0.32, t) * (1.0 - smoothstep(2.45, 3.10, t));
    float frameDistance = abs(dot((SCREEN_UV - vec2(0.5)) * metric, normal));
    float edge = 0.64;
    float wedge = smoothstep(edge, edge + 0.003, frameDistance) * frame;
    color = mix(color, vec3(0.005), wedge);
    float gold = exp(-abs(frameDistance - edge) * 1100.0) * frame;
    color = mix(color, vec3(0.65, 0.55, 0.34), gold * 0.35);
    COLOR = vec4(clamp(color, vec3(0.0), vec3(1.0)), 1.0);
}
