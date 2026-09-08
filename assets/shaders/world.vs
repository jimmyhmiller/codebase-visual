#version 330
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;
layout(location = 8) in mat4 instanceTransform;
uniform int useInstances = 0;
uniform mat4 mvp;
uniform mat4 lightVP;
// Cached glyph vertices are local in XZ; immediate geometry uses zero offset.
uniform vec3 glyphOffset;
uniform float glyphScale = 1.0;
out vec3 worldPosition;
out vec3 normal;
out vec4 color;
out vec4 lightPosition;
void main() {
    vec4 position = vec4(vertexPosition, 1.0);
    if (useInstances != 0) {
        position = instanceTransform * position;
        worldPosition = position.xyz;
        normal = transpose(inverse(mat3(instanceTransform))) * vertexNormal;
    } else {
        worldPosition = vertexPosition * glyphScale + glyphOffset;
        normal = vertexNormal;
    }
    color = vertexColor;
    lightPosition = lightVP * vec4(worldPosition, 1.0);
    gl_Position = mvp * position;
}
