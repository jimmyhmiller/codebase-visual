#version 330
in vec3 worldPosition;
uniform float terrainMaterial;
uniform float clipHeight;
out vec4 finalColor;
void main() {
    if (terrainMaterial < 0.5 && worldPosition.y > clipHeight) discard;
    vec3 encoded = fract(gl_FragCoord.z * vec3(1.0, 255.0, 65025.0));
    encoded -= encoded.yzz * vec3(1.0/255.0, 1.0/255.0, 0.0);
    finalColor = vec4(encoded, 1.0);
}
