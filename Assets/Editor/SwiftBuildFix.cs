using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class SwiftBuildFix
{
   
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget!= BuildTarget.iOS) return;

        // Caminho do projeto Xcode
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // Identifica o alvo UnityFramework (onde ocorre o erro)
        string targetGuid = proj.GetUnityFrameworkTargetGuid();

        // 1. Configura para NÃO embutir bibliotecas Swift no Framework (evita rejeição na Apple Store)
        proj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

        // 2. Cria um arquivo Swift vazio para forçar o Xcode a carregar as bibliotecas de compatibilidade
        string swiftFilePath = Path.Combine(path, "SwiftFix.swift");
        File.WriteAllText(swiftFilePath, "// Arquivo vazio para forcar o link do Swift\nimport Foundation");

        // Adiciona o arquivo ao projeto e ao alvo UnityFramework
        string fileGuid = proj.AddFile(swiftFilePath, "SwiftFix.swift", PBXSourceTree.Source);
        proj.AddFileToBuild(targetGuid, fileGuid);

        // Salva as alterações
        proj.WriteToFile(projPath);
        
        Debug.Log("SwiftBuildFix: Correção de Swift aplicada ao UnityFramework.");
    }
}