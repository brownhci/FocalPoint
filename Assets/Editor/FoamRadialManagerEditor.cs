using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FoamRadialManager))]
public class FoamRadialManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //The target variable is the selected MyBehaviour.
        FoamRadialManager frm = (FoamRadialManager)target;

        //Create a toggle for the bool variable;
        frm.IsToolOption = EditorGUILayout.Toggle("Is Too lOption", frm.IsToolOption);

        //Create an input field for the int only if the bool field is true.
        if (frm.IsToolOption)
        {
            //frm.m_inUseSprite = (Sprite)EditorGUILayout.ObjectField("InUseSprite", frm.m_inUseSprite, typeof(Sprite), allowSceneObjects: true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("In Use State Icon");
            frm.m_inUseSprite = (Sprite)EditorGUILayout.ObjectField(frm.m_inUseSprite, typeof(Sprite), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();
            frm.m_iconChild = (SpriteRenderer)EditorGUILayout.ObjectField("Icon Renderer", frm.m_iconChild, typeof(SpriteRenderer), allowSceneObjects: true);
        }
    }
}