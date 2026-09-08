#version 330
in vec3 vertexPosition;
in vec4 vertexColor;
in vec2 vertexTexCoord;
uniform mat4 mvp;
out vec3 waterPosition;
out vec4 waterColor;
out vec2 shelfCoordinate;
void main() {
    waterPosition = vertexPosition;
    waterColor = vertexColor;
    shelfCoordinate = vertexTexCoord;
    gl_Position = mvp * vec4(vertexPosition, 1.0);
}
