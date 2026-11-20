
// using UnityEditor;
// using UnityEngine;
// using Pokemon.World;
// using Pokemon.Visual;

// namespace Pokemon.EditorTools
// {
//     public class EditorMapVisualizer : EditorWindow
//     {
//         private Map map;
//         private Texture2D previewTexture;

//         [MenuItem("Tools/Pokemon/Map Preview")]
//         public static void OpenWindow()
//         {
//             GetWindow<EditorMapVisualizer>("Map Preview");
//         }

//         private void OnGUI()
//         {
//             EditorGUILayout.LabelField(
//                 "Map Visualizer",
//                 EditorStyles.boldLabel
//             );

//             EditorGUILayout.Space();

//             if (GUILayout.Button("Generate Example Map"))
//             {
//                 map = new Map(MapExample.Generate());
//                 ClearTexture();
//             }

//             GUI.enabled = map != null;

//             if (GUILayout.Button("Run Visualization"))
//             {
//                 ClearTexture();

//                 previewTexture =
//                     MapVisualizer.RunVisualization(map);
//             }

//             GUI.enabled = true;

//             if (GUILayout.Button("Clear"))
//             {
//                 ClearTexture();
//                 map = null;
//             }

//             EditorGUILayout.Space();

//             if (previewTexture != null)
//             {
//                 float availableWidth =
//                     EditorGUIUtility.currentViewWidth - 30f;

//                 float scale = Mathf.Min(
//                     availableWidth / previewTexture.width,
//                     32f
//                 );

//                 float size = previewTexture.width * scale;

//                 Rect rect = GUILayoutUtility.GetRect(
//                     size,
//                     size
//                 );

//                 GUI.DrawTexture(
//                     rect,
//                     previewTexture,
//                     ScaleMode.ScaleToFit
//                 );
//             }
//         }

//         private void ClearTexture()
//         {
//             if (previewTexture != null)
//             {
//                 DestroyImmediate(previewTexture);
//                 previewTexture = null;
//             }
//         }

//         private void OnDisable()
//         {
//             ClearTexture();
//         }
//     }
// }
