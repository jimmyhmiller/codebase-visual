#version 330
in vec3 waterPosition;
in vec4 waterColor;
in vec2 shelfCoordinate;
uniform float waterTime;
uniform vec3 waterEye;
out vec4 finalColor;

// Analytic wave slopes; attenuation integrates out frequencies smaller than a
// pixel instead of letting specular highlights shimmer when the camera zooms.
vec2 slope(vec2 p, vec2 direction, float frequency, float speed, float amplitude) {
    vec2 crossDirection = vec2(-direction.y, direction.x);
    float crossPhase = dot(p, crossDirection) * 0.37;
    float phase = dot(p, direction) * frequency + 1.8 * sin(crossPhase);
    vec2 phaseSlope = direction + crossDirection * (0.666 * cos(crossPhase) / frequency);
    float footprint = length(vec2(dFdx(phase), dFdy(phase)));
    float visible = exp(-0.5 * footprint * footprint);
    return phaseSlope * cos(phase + waterTime * speed) * amplitude * visible;
}

void main() {
    vec2 p = waterPosition.xz;
    vec2 gradient = slope(p, vec2(0.94, 0.342), 1.7, 0.45, 0.065)
                  + slope(p, vec2(-0.6, 0.8), 3.1, -0.62, 0.04)
                  + slope(p, vec2(0.8, 0.6), 7.3, 0.91, 0.025)
                  + slope(p, vec2(-0.28, 0.96), 13.0, -1.15, 0.012);
    vec3 normal = normalize(vec3(-gradient.x, 1.0, -gradient.y));
    vec3 eye = normalize(waterEye);
    vec3 reflected = reflect(-eye, normal);
    float grazing = pow(1.0 - max(dot(normal, eye), 0.0), 5.0);
    float sky = smoothstep(-0.2, 0.8, reflected.y);
    vec3 reflection = mix(vec3(0.07, 0.12, 0.14), vec3(0.16, 0.23, 0.25), sky);
    vec3 halfLight = normalize(eye + normalize(vec3(-0.5, 0.8, -0.35)));
    float glint = pow(max(dot(normal, halfLight), 0.0), 90.0);
    float current = sin(p.x * 0.19 + sin(p.y * 0.13)) * sin(p.y * 0.17 - p.x * 0.09);
    vec3 body = waterColor.rgb * (0.96 + current * 0.045 + gradient.x * 0.7);
    vec3 color = mix(body, reflection, 0.045 + grazing * 0.32);
    color += vec3(0.012, 0.016, 0.018) * glint;
    // Shelf color/opacity is a sampled regional field, not radial rings.
    finalColor = vec4(color, waterColor.a);
}
