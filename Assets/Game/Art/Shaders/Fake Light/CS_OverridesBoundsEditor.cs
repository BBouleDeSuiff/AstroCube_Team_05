using UnityEditor; 
using UnityEngine; 

[ CustomEditor(typeof(OverrideBounds)) ] 
public  class  OverrideBoundsEditor : Editor
 { 
    public  override  void  OnInspectorGUI ()
     { 
        var component = (OverrideBounds)target; 
        DrawDefaultInspector(); 

        if (GUILayout.Button( "Create Updated Meshes Bounds" )) 
        { 
            component.CreateUpdatedMesh(); 
        } 
    } 
}