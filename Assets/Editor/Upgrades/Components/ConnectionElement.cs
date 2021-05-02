using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class ConnectionElement : VisualElement
    {
        public int SortOrder = 10;
        
        private readonly Node output;
        private readonly Node input;
        
        private readonly TechTree tree;

        private Vector2 mousePos;
        
        public ConnectionElement(Node output, Node input, Color color, float thickness, bool isGlowEnabled)
        {
            this.output = output;
            this.input = input;
            
            generateVisualContent = (ctx) => DrawLine(
                ctx,
                output.GetConnectingPosition(input.GetPosition()),
                input.GetConnectingPosition(output.GetPosition()),
                color,
                thickness,
                isGlowEnabled
            );

            output.OnMove += MarkDirtyRepaint;
            input.OnMove += MarkDirtyRepaint;
        }

        public ConnectionElement(TechTree tree, Vector2 startPos, Color color, float thickness, bool isGlowEnabled)
        {
            this.tree = tree;
            
            mousePos = startPos;
            
            generateVisualContent = ctx =>
            {
                DrawLine(
                    ctx,
                    startPos,
                    mousePos,
                    color,
                    thickness,
                    isGlowEnabled
                );
            };

            tree.OnDrag += TechTreeMouseMoved;
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

            var length = thickness * 4f;
            var width = thickness * 3f;

            var distAlongLine = 1f;
            
            var dir = (endPos - startPos).normalized;
            var dist = (endPos - startPos).magnitude;
            var center = new Vector3(startPos.x + dir.x * (dist * distAlongLine - length), startPos.y + dir.y * (dist * distAlongLine -  length), Vertex.nearZ);
            var normal = new Vector3(-dir.y, dir.x, 0f);
            
            var vertices = new Vertex[3];
            vertices[0].position = center + normal * width;
            vertices[1].position = center - normal * width;
            vertices[2].position = new Vector3(center.x + dir.x * length, center.y + dir.y * length, center.z);
            
            vertices[0].tint = color;
            vertices[1].tint = color;
            vertices[2].tint = color;
            
            mesh.SetAllVertices( vertices );
            mesh.SetAllIndices(new ushort[] { 0, 1, 2 });
        }

        ~ConnectionElement()
        {
            if (output != null) output.OnMove -= MarkDirtyRepaint;
            if (input != null) input.OnMove -= MarkDirtyRepaint;
            if (tree != null) tree.OnDrag -= TechTreeMouseMoved;
        }

        private void TechTreeMouseMoved(Vector2 mousePos)
        {
            this.mousePos = mousePos;
            MarkDirtyRepaint();
        }
    }
}