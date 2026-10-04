using UnityEngine; 

public  class  OverrideBounds : MonoBehaviour
 { 
    public Vector3 Center; 
    public Vector3 Size; 
    
    public  void  CreateUpdatedMesh ()
     { 
        var meshFilter = GetComponent<MeshFilter>(); 
        var meshCopy = Instantiate(meshFilter.sharedMesh); 
        meshCopy.bounds = new Bounds (Center, Size); 
        meshFilter.sharedMesh = meshCopy; 
    } 
}
