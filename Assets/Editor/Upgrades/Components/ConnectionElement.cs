using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class ConnectionElement : VisualElement
    {
        private readonly Node output;
        private readonly Node input;
        
        public ConnectionElement(Node output, Node input, Color color, float thickness, bool isGlowEnabled)
        {
            this.output = output;
            this.input = input;
            
            generateVisualContent = (ctx) => DrawLine(
                ctx,
                output.GetOutputPosition(),
                input.GetInputPosition(),
                color,
                thickness,
                isGlowEnabled
            );

            output.OnMove += MarkDirtyRepaint;
            input.OnMove += MarkDirtyRepaint;
        }
        
        private void DrawLine(MeshGenerationContext ctx, Vector2 startPos, Vector2 endPos, Color color, float thickness, bool isGlowEnabled)
        {
            if (isGlowEnabled)
            {
                var colorBase = new Color(color.r, color.g, color.b, color.a * 0.3f);
                var colorNear = new Color(color.r, color.g, color.b, color.a * 0.5f);
                DrawLine(ctx, startPos, endPos, colorBase, thickness * 3f);
                DrawLine(ctx, startPos, endPos, colorNear, thickness * 1.5f);
            }
            
            DrawLine(ctx, startPos, endPos, color, thickness);
            DrawTriangle(ctx, startPos, endPos, color, thickness);
        }

        private void DrawLine(MeshGenerationContext ctx, Vector2 startPos, Vector2 endPos, Color color, float thickness)
        {
            var mesh = ctx.Allocate( 6, 6 );
            
            var dir = (endPos - startPos).normalized;

            var startCenter = new Vector3(startPos.x, startPos.y, Vertex.nearZ);
            var endCenter = new Vector3(endPos.x, endPos.y, Vertex.nearZ);
            var normal = new Vector3(-dir.y, dir.x, 0f);

            var halfThickness = thickness * 0.5f;
            
            var vertices = new Vertex[6];
            vertices[0].position = startCenter + normal * halfThickness;
            vertices[1].position = startCenter - normal * halfThickness;
            vertices[2].position = endCenter + normal * halfThickness;
            
            vertices[3].position = startCenter - normal * halfThickness;
            vertices[4].position = endCenter - normal * halfThickness;
            vertices[5].position = endCenter + normal * halfThickness;
            
            vertices[0].tint = color;
            vertices[1].tint = color;
            vertices[2].tint = color;
            vertices[3].tint = color;
            vertices[4].tint = color;
            vertices[5].tint = color;
            
            mesh.SetAllVertices( vertices );
            mesh.SetAllIndices( new ushort[]{ 0, 1, 2, 3, 4, 5 });
        }

        private void DrawTriangle(MeshGenerationContext ctx, Vector2 startPos, Vector2 endPos, Color color, float thickness)
        {
            var mesh = ctx.Allocate( 3, 3 );

            var halfLength = thickness * 2f;
            var width = thickness * 3f;
            
            var dir = (endPos - startPos).normalized;
            var dist = (endPos - startPos).magnitude;
            var center = new Vector3(startPos.x + dir.x * (dist * 0.5f - halfLength), startPos.y + dir.y * (dist * 0.5f - halfLength), Vertex.nearZ);
            var normal = new Vector3(-dir.y, dir.x, 0f);
            
            var vertices = new Vertex[3];
            vertices[0].position = center + normal * width;
            vertices[1].position = center - normal * width;
            vertices[2].position = new Vector3(center.x + dir.x * halfLength * 2f, center.y + dir.y * thickness * 4f, center.z);
            
            vertices[0].tint = color;
            vertices[1].tint = color;
            vertices[2].tint = color;
            
            mesh.SetAllVertices( vertices );
            mesh.SetAllIndices(new ushort[] { 0, 1, 2 });
        }

        ~ConnectionElement()
        {
            output.OnMove -= MarkDirtyRepaint;
            input.OnMove -= MarkDirtyRepaint;
        }
    }
}