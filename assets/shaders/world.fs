#version 330
in vec3 worldPosition;
in vec3 normal;
in vec4 color;
in vec4 lightPosition;
uniform sampler2D shadowMap;
uniform vec4 colDiffuse;
uniform float terrainMaterial;
uniform vec3 viewDirection;
uniform float clipHeight;
uniform float blueprint;
out vec4 finalColor;
float noise(vec3 p) {
    return fract(sin(dot(p, vec3(12.9898,78.233,37.719))) * 43758.5453);
}
// Continuous world-space stone texture: no UV seams between strata or rocks.
float stoneNoise(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    return mix(mix(mix(noise(i), noise(i+vec3(1,0,0)), f.x),
                   mix(noise(i+vec3(0,1,0)), noise(i+vec3(1,1,0)), f.x), f.y),
               mix(mix(noise(i+vec3(0,0,1)), noise(i+vec3(1,0,1)), f.x),
                   mix(noise(i+vec3(0,1,1)), noise(i+vec3(1,1,1)), f.x), f.y), f.z);
}
// Value-noise derivative evaluated analytically. This replaces six extra
// height evaluations per octave and keeps the relief stable across zoom.
vec4 stoneGradient(vec3 p) {
    vec3 cell = floor(p), f = fract(p);
    vec3 u = f*f*(3.0-2.0*f), du = 6.0*f*(1.0-f);
    float a = noise(cell), b = noise(cell+vec3(1,0,0));
    float c = noise(cell+vec3(0,1,0)), d = noise(cell+vec3(1,1,0));
    float e = noise(cell+vec3(0,0,1)), g = noise(cell+vec3(0,1,1));
    float h = noise(cell+vec3(1,1,1)), j = noise(cell+vec3(1,0,1));
    float k1 = b-a, k2 = c-a, k3 = e-a;
    float k4 = a-b-c+d, k5 = a-c-e+g, k6 = a-b-e+j;
    float k7 = -a+b+c-d+e-j-g+h;
    float value = a+k1*u.x+k2*u.y+k3*u.z+k4*u.x*u.y+
                  k5*u.y*u.z+k6*u.z*u.x+k7*u.x*u.y*u.z;
    vec3 gradient = du*vec3(k1+k4*u.y+k6*u.z+k7*u.y*u.z,
                            k2+k4*u.x+k5*u.z+k7*u.x*u.z,
                            k3+k5*u.y+k6*u.x+k7*u.x*u.y);
    return vec4(value,gradient);
}
// World-space mineral cells cross cliff/terrace boundaries without UV seams.
// F2-F1 gives a fine fracture network; the second component varies each chip.
vec2 mineralCells(vec3 p) {
    vec3 cell = floor(p), f = fract(p);
    float nearest = 100.0, second = 100.0, chip = 0.0;
    for (int z=-1; z<=1; ++z)
      for (int y=-1; y<=1; ++y)
        for (int x=-1; x<=1; ++x) {
            vec3 offset = vec3(x,y,z), q = cell+offset;
            vec3 jitter = fract(sin(vec3(dot(q,vec3(127.1,311.7,74.7)),
                                          dot(q,vec3(269.5,183.3,246.1)),
                                          dot(q,vec3(113.5,271.9,124.6))))*43758.5453);
            vec3 delta = offset+0.15+0.7*jitter-f;
            float distance = dot(delta,delta);
            if (distance < nearest) {
                second = nearest; nearest = distance; chip = jitter.x;
            } else second = min(second,distance);
        }
    return vec2(sqrt(second)-sqrt(nearest),chip);
}
// GGX conductor response with correlated Smith visibility and Schlick Fresnel.
// Roughness is perceptual; squaring it produces the microfacet slope width.
vec3 metalSpecular(vec3 n, vec3 eye, vec3 light, vec3 f0, float roughness) {
    vec3 h = normalize(eye+light);
    float nv = max(dot(n,eye),0.0001), nl = max(dot(n,light),0.0);
    float nh = max(dot(n,h),0.0), vh = max(dot(eye,h),0.0);
    float alpha = roughness*roughness, a2 = alpha*alpha;
    float denominator = nh*nh*(a2-1.0)+1.0;
    float distribution = a2/(3.14159265*denominator*denominator);
    float masking = 0.5/max(nl*sqrt(nv*nv*(1.0-a2)+a2)
                           +nv*sqrt(nl*nl*(1.0-a2)+a2),0.0001);
    vec3 fresnel = f0+(vec3(1.0)-f0)*pow(1.0-vh,5.0);
    return distribution*masking*fresnel*nl;
}
void main() {
    if (terrainMaterial < 0.5 && worldPosition.y > clipHeight) discard;
    if (terrainMaterial < 0.5 && blueprint > 0.5) {
        finalColor = vec4(color.rgb * 1.3, color.a * 0.72);
        return;
    }
    vec3 n = normalize(normal);
    vec3 base = color.rgb * colDiffuse.rgb;
    float wetness = 0.0;
    if (terrainMaterial > 0.5) {
        vec3 p = worldPosition;
        float exposedShelf = smoothstep(0.55,0.90,n.y)
                           * (1.0-smoothstep(1.75,1.94,p.y));
        float broad = stoneNoise(p*0.65);
        vec4 coarse = stoneGradient(p*2.8), fine = stoneGradient(p*9.0);
        // Integrate out subpixel bump octaves before perturbing the normal.
        // Albedo filtering alone cannot prevent shimmering normal highlights.
        float coarseFootprint = length(fwidth(p*2.8));
        float fineFootprint = length(fwidth(p*9.0));
        float coarseVisibility = exp(-0.5*coarseFootprint*coarseFootprint);
        float fineVisibility = exp(-0.5*fineFootprint*fineFootprint);
        vec3 gradient = coarse.yzw*(2.8*0.7*coarseVisibility)
                      +fine.yzw*(9.0*0.3*fineVisibility);
        n = normalize(n - (gradient-n*dot(n,gradient))*0.085);
        vec3 mineralPosition = p*vec3(16.0,34.0,16.0);
        vec2 mineral = mineralCells(mineralPosition);
        float footprint = length(fwidth(mineralPosition));
        float detailVisibility = 1.0-smoothstep(0.30,0.95,footprint);
        float edgeAA = max(fwidth(mineral.x),0.006);
        float fracture = 1.0-smoothstep(0.012-edgeAA,0.025+edgeAA,mineral.x);
        float sediment = 0.5+0.5*sin(p.y*22.0+stoneNoise(p*1.7)*5.0);
        float moss = smoothstep(0.57,0.78,broad)*smoothstep(0.60,0.95,n.y);
        float stoneValue = dot(base,vec3(0.2126,0.7152,0.0722));
        // Dark, slightly cool stone separates the terrain from the metal
        // architecture. Weathered upward-facing coastal shelves retain ochre.
        base = mix(base,stoneValue*vec3(0.86,0.96,0.95),0.70)*0.72;
        base *= mix(vec3(1.0),vec3(1.48,1.25,0.78),
                    exposedShelf*mix(0.45,0.85,broad));
        base *= mix(0.82,1.12,broad)*mix(0.92,1.08,coarse.x);
        base *= mix(1.0,mix(0.92,1.08,mineral.y),detailVisibility);
        base *= 1.0-0.18*fracture*detailVisibility*smoothstep(0.25,0.65,coarse.x);
        base *= mix(0.95,1.04,sediment);
        base = mix(base,base*vec3(0.78,1.04,0.68),moss*0.48);
        // A narrow, irregular splash zone ties exposed outcrops and lower
        // cliffs to the ocean. Upper plateau materials remain dry.
        wetness = 1.0-smoothstep(-0.42,-0.12,p.y+(coarse.x-0.5)*0.09);
        base *= mix(vec3(1.0),vec3(0.55,0.62,0.64),wetness);
    }
    vec3 light = normalize(vec3(-0.6, 1.0, -0.7));
    vec3 projected = lightPosition.xyz / lightPosition.w * 0.5 + 0.5;
    float visibility = 1.0;
    if (projected.x > 0.0 && projected.x < 1.0 && projected.y > 0.0 && projected.y < 1.0) {
        float shadow = 0.0;
        float bias = max(0.00025, 0.0008 * (1.0 - dot(n, light)));
        for (int x = -1; x <= 1; ++x)
            for (int y = -1; y <= 1; ++y) {
                float depth = dot(texture(shadowMap, projected.xy + vec2(x,y)/2048.0),
                                  vec4(1.0, 1.0/255.0, 1.0/65025.0, 0.0));
                shadow += projected.z - bias > depth ? 1.0 : 0.0;
            }
        visibility = 1.0 - shadow / 9.0 * 0.72;
    }
    float grainVisibility = 1.0-smoothstep(0.4,1.0,length(fwidth(worldPosition))*35.0);
    float grain = mix(1.0,mix(0.96,1.03,noise(floor(worldPosition*35.0))),grainVisibility);
    float diffuse = max(dot(n, light), 0.0);
    float ambient = 0.45 + max(n.y, 0.0) * 0.19;
    vec3 lit = base * (ambient + diffuse * visibility * 0.75) * grain;
    vec3 eye = normalize(viewDirection);
    vec3 halfVector = normalize(light + eye);
    if (terrainMaterial > 0.5) {
        float specular = pow(max(dot(n,halfVector),0.0),mix(12.0,48.0,wetness));
        lit += vec3(0.86,0.88,0.81)*specular*visibility*mix(0.035,0.14,wetness);
    } else {
        // Brushed relief is tangent to the actual surface. Fade it before
        // subpixel machining marks would shimmer during camera movement.
        vec3 machiningScale = vec3(110.0,7.0,13.0);
        vec3 machining = worldPosition*machiningScale;
        float machiningVisibility = 1.0-smoothstep(0.3,0.9,
                                                  length(fwidth(machining)));
        vec4 finishNoise = stoneGradient(machining);
        vec3 finishGradient = finishNoise.yzw*machiningScale;
        n = normalize(n-(finishGradient-n*dot(n,finishGradient))
                        *(0.0015*machiningVisibility));
        // The analytic lighting environment is world-oriented, not a scene
        // reflection. Metal gets most of its energy from reflected light,
        // with a small diffuse residue for the weathered/coated finish.
        vec3 reflection = reflect(-eye,n);
        float sky = smoothstep(-0.25,0.8,reflection.y);
        // Broad reflected ground/bounce light keeps vertical metal legible.
        // A low studio fill is world-oriented like the upper softboxes; it
        // reveals folded faces without a camera-facing or unlit brightness floor.
        vec3 environment = mix(vec3(0.12,0.15,0.17),
                               vec3(0.34,0.40,0.42),sky);
        float warmPanel = pow(max(dot(reflection,
                                      normalize(vec3(-0.6,0.9,-0.7))),0.0),8.0);
        float coolPanel = pow(max(dot(reflection,
                                      normalize(vec3(0.8,0.35,0.4))),0.0),5.0);
        float lowPanel = pow(max(dot(reflection,
                                     normalize(vec3(-0.4,-0.45,0.8))),0.0),3.0);
        environment += vec3(0.82,0.73,0.55)*warmPanel;
        environment += vec3(0.20,0.36,0.40)*coolPanel;
        environment += vec3(0.42,0.46,0.48)*lowPanel;
        float grazing = pow(1.0-max(dot(n,eye),0.0),5.0);
        vec3 f0 = clamp(base,vec3(0.02),vec3(0.95));
        vec3 reflectance = mix(f0,vec3(1.0),grazing);
        float finish = mix(1.0,mix(0.93,1.04,finishNoise.x),
                           machiningVisibility);
        float roughness = mix(0.50,0.56,finishNoise.x*machiningVisibility);
        lit = base*(ambient*0.30+diffuse*visibility*0.12)*grain;
        lit += reflectance*environment*finish*(0.50+0.50*visibility);
        lit += metalSpecular(n,eye,light,f0,roughness)
               *vec3(0.55,0.52,0.47)*visibility;
    }
    // Copper and mint materials retain a subdued emissive core.
    float amber = smoothstep(0.09, 0.28, min(base.r - base.b, base.r - base.g)) * step(0.48, base.r);
    lit = lit * mix(1.0, 0.85, amber) + base * amber * 0.08;
    if (terrainMaterial < 0.5) {
        float rim = 1.0-smoothstep(0.0,0.065,abs(worldPosition.y-clipHeight));
        lit += base*rim*0.7;
    }
    finalColor = vec4(lit, color.a * colDiffuse.a);
}
