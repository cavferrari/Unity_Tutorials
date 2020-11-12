using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MyGrid : MonoBehaviour
{
	
	public int xSize, ySize;
	
	private Vector3[] vertices;
	/*(7)We only really need a reference to the mesh inside the Generate method. 
	 *   As the mesh filter has a reference to it as well, it will stick around anyway. 
	 *   I made it a global variable because the next logical step beyond this tutorial would be to animate the mesh, which I encourage you to try.
	 */
	private Mesh mesh;
	
	private void Awake () 
	{
		Generate();
	}
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /*(2)Let's visualize these vertices so we can check that we position them correctly. 
     *   We can do so by adding an OnDrawGizmos method and drawing a small black sphere in the scene view for every vertex.
     */
	private void OnDrawGizmos () 
	{
		/*(4)This will produce errors when we are not in play mode, because OnDrawGizmos methods are also invoked while Unity 
		 *   is in edit mode, when we don't have any vertices. To prevent this error, check whether the array exists and jump out 
		 *   of the method if it isn't.
		 */
		if (vertices == null) 
		{
			return;
		}
		/*(3)Gizmos are visual cues that you can use in the editor. By default they're visible in the scene view and not in the game view, 
		 *   but you can adjust this via their toolbars. The Gizmos utility class allows you to draw icons, lines, and some other things.
		 *   Gizmos can be drawn inside an OnDrawGizmos method, which is automatically invoked by the Unity editor. 
		 *   An alternative method is OnDrawGizmosSelected, which is only invoked for selected objects.
		 */
		Gizmos.color = Color.black;
		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.DrawSphere(vertices[i], 0.1f);
		}
	}    
   
	private void Generate () 
	{	
		/*(1)Let's focus on the vertex positions first and leave the triangles for later. 
		 *   We need to hold an array of 3D vectors to store the points. The amount of vertices depends on the size of the grid. 
		 *   We need a vertex at the corners of every quad, but adjacent quads can share the same vertex. 
		 *   So we need one more vertex than we have tiles in each dimension.(#x+1)(#y+1)
		 */
		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		/*(16)Next up are the UV coordinates. You might have noticed that the grid currently has a uniform color, even though it uses a material 
		 *    with an albedo texture. This makes sense, because if we don't provide the UV coordinates ourselves then they're all zero.
		 */
		Vector2[] uv = new Vector2[vertices.Length];
		/*(17)Another way to add more apparent detail to a surface is to use a normal map. These maps contain normal vectors encoded as colors. 
		 *    Applying them to a surface will result in much more detailed light effects than could be created with vertex normals alone.
		 *    Applying this material to our grid doesn't give us any bumps yet. We need to add tangent vectors to our mesh first.
		 */
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		/*(5)While in play mode, we see only a single sphere at the origin. This is because we haven't positioned the vertices yet,
		 *   so they all overlap at that position. We have to iterate through all positions, using a double loop. 
		 *   Gizmos are drawn directly in world space, not in the object's local space. If you want them to respect your objects transform,
		 *   you'll have to explicitly apply it by using transform.TransformPoint(vertices[i]) instead of just vertices[i].
		 */
		for (int i = 0, y = 0; y <= ySize; y++) 
		{
			for (int x = 0; x <= xSize; x++, i++) 
			{
				vertices[i] = new Vector3(x, y);
				/*(16)To make the texture to fit our entire grid, simply divide the position of the vertex by the grid dimensions.
				uv[i] = new Vector2(x / xSize, y / ySize);*/
				/*(17)The texture shows up now, but it's not covering the entire grid. Its exact appearance depends on whether the
				 *    texture's wrap mode is set to clamp or repeat. This happens because we're currently dividing integers by integers, 
				 *    which results in another integer. To get the correct coordinates between zero and one across the entire grid, 
				 *    we have to make sure that we're using floats. The texture is now projected onto the entire grid. 
				 *    As I've set the grid's size to ten by five, the texture will appear stretched horizontally.
				 *    This can be countered by adjusting the texture's tiling settings of the material. 
				 *    By settings it to (2, 1) the U coordinates will be doubled. If the texture is set to repeat, then we'll see two square tiles of it.
				 */
				uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
				//(17) As we have a flat surface, all tangents simply point in the same direction, which is to the right.
				tangents[i] = tangent;
			}
		}
		/*(7)Now that we know that the vertices are positioned correctly, we can deal with the actual mesh. 
		 *   Besides holding a reference to it in our own component, we must also assign it to the mesh filter. 
		 *   Then once we dealt with the vertices, we can give them to our mesh.
		 *(8)We now have a mesh in play mode, but it doesn't show up yet because we haven't given it any triangles. 
		 *   Triangles are defined via an array of vertex indices. As each triangle has three points, three consecutive indices describe one triangle. 
		 *   Let's start with just one triangle.
		 *(9)We now have one triangle, but the three points that we are using all lie in a straight line. 
		 *	 This produces a degenerate triangle, which isn't visible. The first two vertices are fine, 
		 *   but then we should jump to the first vertex of the next row.
		 *(10)Which side a triangle is visible from is determined by the orientation of its vertex indices. By default, if they are arranged in a 
		 *	  clockwise direction the triangle is considered to be forward-facing and visible. Counter-clockwise triangles are discarded so we 
		 *    don't need to spend time rendering the insides of objects, which are typically not meant to be seen anyway.
		 *    So to make the triangle appear when we look down the Z axis, we have to change the order in which its vertices are traversed. 
		 *    We can do so by swapping the last two indices.
		 *(11)We now have one triangle that covers half of the first tile of our grid. To cover the entire tile, all we need is a second triangle.
		 *(12)We can create the entire first row of tiles by turning this into a loop. 
		 *    As we're iterating over both vertex and triangle indices, we have to keep track of both. 
		 *    Let's also move the yield statement into this loop, so we no longer have to wait for the vertices to appear.
		 */
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid"; 
		mesh.vertices = vertices;
		//(16)
		mesh.uv = uv;
		//(17)
		mesh.tangents = tangents;
		/*(13)Now fill the entire grid by turning the single loop into a double loop. Note that moving to the next row requires incrementing 
		 *    the vertex index by one, because there's one more vertex than tiles per row.
         */
		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) 
		{
			for (int x = 0; x < xSize; x++, ti += 6, vi++) 
			{
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
				/*(14)The vertex gizmos now immediately appear, and the triangles all appear at once after a short wait. 
				 *    To see the tiles appear one by one, we have to update the mesh each iteration, instead of only after the loop.
				 */
				mesh.triangles = triangles;
				/*(15)Our grid is currently lit in a peculiar way. That's because we haven't given any normals to the mesh yet. 
				 *    The default normal direction is (0, 0, 1) which is the exact opposite of what we need.How do normals work?
				 *    A normal is vector that is perpendicular to a surface. We always use normals of unit length and they point 
				 *    to the outside of their surface, not to the inside. Normals can be used to determine the angle at which a 
				 *    light ray hits a surface, if at all. The specifics of how it is used depends on the shader.
				 *    As a triangle is always flat, there shouldn't be a need to provide separate information about normals. 
				 *    However, by doing so we can cheat. In reality vertices don't have normals, triangles do. 
				 *    By attaching custom normals to vertices and interpolating between them across triangles, 
				 *    we can pretend that we have a smoothly curving surface instead of a bunch of flat triangles. 
				 *    This illusion is convincing, as long as you don't pay attention to the sharp silhouette of the mesh.
				 *    Normals are defined per vertex, so we have to fill another vector array. 
				 *    Alternatively, we can ask the mesh to figure out the normals itself based on its triangles. Let's be lazy this time and do that.
				 */
				mesh.RecalculateNormals();
			}
		}
	}   
}
