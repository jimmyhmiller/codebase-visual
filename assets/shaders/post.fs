#version 330
in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;
uniform sampler2D sceneDepth;
// Actual orthographic projection: m0, m5, m10, m14.
uniform vec4 projection;
uniform vec4 colDiffuse;
out vec4 finalColor;

vec3 viewPosition(vec2 uv) {
    float depth = texture(sceneDepth, uv).r;
    return vec3((uv*2.0-1.0)/projection.xy,
                (depth*2.0-1.0-projection.w)/projection.z);
}

// World-scale screen-space contact obscurance. It cannot see hidden surfaces;
// distance rejection prevents unrelated foreground silhouettes casting halos.
float contactVisibility(vec2 uv, vec2 texel) {
    if (texture(sceneDepth, uv).r >= 0.999999) return 1.0;
    vec3 p = viewPosition(uv);
    vec3 xp = viewPosition(uv+vec2(texel.x,0.0))-p;
    vec3 xm = p-viewPosition(uv-vec2(texel.x,0.0));
    vec3 yp = viewPosition(uv+vec2(0.0,texel.y))-p;
    vec3 ym = p-viewPosition(uv-vec2(0.0,texel.y));
    vec3 dx = abs(xp.z) < abs(xm.z) ? xp : xm;
    vec3 dy = abs(yp.z) < abs(ym.z) ? yp : ym;
    vec3 n = normalize(cross(dx,dy));
    if (n.z < 0.0) n = -n;
    const float radius = 0.70;
    float obscurance = 0.0;
    for (int i=0; i<24; ++i) {
        float angle = float(i)*2.39996323;
        float distance = radius*sqrt((float(i)+0.5)/24.0);
        vec2 sampleUV = uv + vec2(cos(angle),sin(angle))*distance*projection.xy*0.5;
        if (any(lessThan(sampleUV,vec2(0.0))) || any(greaterThan(sampleUV,vec2(1.0)))) continue;
        vec3 delta = viewPosition(sampleUV)-p;
        float d2 = dot(delta,delta);
        // World-space bias also rejects depth-quantization self-occlusion.
        float horizon = max((dot(n,delta)-0.015)*inversesqrt(max(d2,0.000001))-0.08,0.0);
        obscurance += horizon*max(1.0-d2/(radius*radius),0.0);
    }
    return clamp(1.0-obscurance*(2.8/24.0),0.45,1.0);
}
void main() {
    vec2 texel = 1.0 / vec2(textureSize(texture0, 0));
    vec3 base = texture(texture0, fragTexCoord).rgb;
    base *= contactVisibility(fragTexCoord,texel);
    vec3 bloom = vec3(0.0);
    float weights = 0.0;
    for (int x = -3; x <= 3; x++) {
        for (int y = -3; y <= 3; y++) {
            float w = exp(-float(x*x+y*y)/5.0);
            vec3 c = texture(texture0, fragTexCoord + vec2(x,y)*texel*2.0).rgb;
            float amber = smoothstep(0.08, 0.23, min(c.r - c.b, c.r - c.g)) * smoothstep(0.40, 0.75, c.r);
            bloom += c * amber * w;
            weights += w;
        }
    }
    vec2 p = fragTexCoord * 2.0 - 1.0;
    float vignette = 1.0 - 0.10 * dot(p,p);
    vec3 hdr = (base + bloom / weights * 0.45) * vignette;
    // A hue-preserving shoulder keeps copper highlights copper: independently
    // clipping red and green turns the selected slab into a flat yellow patch.
    float peak = max(hdr.r,max(hdr.g,hdr.b));
    float shoulder = 0.75 + 0.25*(1.0-exp(-max(peak-0.75,0.0)/0.25));
    if (peak > 0.75) hdr *= shoulder/peak;
    finalColor = vec4(hdr, 1.0);
}
