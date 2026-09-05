//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System.Linq;
//using System.Collections.Generic;

//public class MaterialCounter
//{
//    [MenuItem("Tools/Contar Materiales en Escena")]
//    static void CountMaterials()
//    {
//        Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true);
//        Dictionary<Material, List<string>> usos = new Dictionary<Material, List<string>>();

//        foreach (var r in renderers)
//        {
//            foreach (var m in r.sharedMaterials)
//            {
//                if (m == null) continue;
//                if (!usos.ContainsKey(m))
//                    usos[m] = new List<string>();
//                usos[m].Add(r.gameObject.name);
//            }
//        }

//        Debug.Log($"Materiales únicos en la escena: {usos.Count}");
//        foreach (var kvp in usos.OrderByDescending(k => k.Value.Count))
//            Debug.Log($" - {kvp.Key.name}: usado por {kvp.Value.Count} objeto(s)");

//        ExportCSV(usos);
//    }

//    static void ExportCSV(Dictionary<Material, List<string>> usos)
//    {
//        string path = EditorUtility.SaveFilePanel("Exportar reporte de materiales", "", "materiales_escena.csv", "csv");
//        if (string.IsNullOrEmpty(path)) return;

//        using (StreamWriter sw = new StreamWriter(path))
//        {
//            sw.WriteLine("Material,Shader,CantidadDeUsuarios,Objetos");
//            foreach (var kvp in usos.OrderByDescending(k => k.Value.Count))
//            {
//                string shader = kvp.Key.shader != null ? kvp.Key.shader.name : "N/A";
//                string objetos = string.Join(" | ", kvp.Value);
//                sw.WriteLine($"\"{kvp.Key.name}\",\"{shader}\",{kvp.Value.Count},\"{objetos}\"");
//            }
//        }

//        Debug.Log($"CSV exportado en: {path}");
//        EditorUtility.RevealInFinder(path);
//    }
//}