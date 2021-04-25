using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.Hex.Upgrades
{
    public class ConnectionElement : VisualElement
    {
        private readonly Node output;
        private readonly Node input;
        
        public ConnectionElement(Node output, Node input)
        {
            this.output = output;
            this.input = input;
            
            generateVisualContent = (ctx) 
                => DrawLine(ctx, output.GetOutputPosition(), input.GetInputPosition(), 2f);

            output.OnMove += MarkDirtyRepaint;
            input.OnMove += MarkDirtyRepaint;
        }
        
        private void DrawLine(MeshGenerationContext cxt, Vector2 startPos, Vector2 endPos, float thickness)
        {
            var mesh = cxt.Allocate( 6, 6 );
            
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
            
            vertices[0].tint = Color.black;
            vertices[1].tint = Color.black;
            vertices[2].tint = Color.black;
            vertices[3].tint = Color.black;
            vertices[4].tint = Color.black;
            vertices[5].tint = Color.black;
            
            mesh.SetAllVertices( vertices );
            mesh.SetAllIndices( new ushort[]{ 0, 1, 2, 3, 4, 5 });
        }

        ~ConnectionElement()
        {
            output.OnMove -= MarkDirtyRepaint;
            input.OnMove -= MarkDirtyRepaint;
        }
    }
}