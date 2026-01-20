# MiSlE
A mod to load custom campaigns on MiSide and MiSide: Zero

## Game file hierarchy

```
MiSide
├── MiSide.exe
└── CustomScenarios
    └── {ScenarioName}
        ├── Languages
        │   └── English
        │       ├── LocationsDescription {ScenarioName}.txt
        │       └── LocationDialogue {ScenarioName}{LevelIndex}.txt
        └── Locations
            └── {ScenarioName}{LevelIndex}.assetbundle
```

Example:

```
MiSide
├── MiSide.exe
└── CustomScenarios
    └── MiSlE
        ├── Languages
        │   └── English
        │       ├── LocationsDescription MiSlE.txt
        │       └── LocationDialogue MiSlE0.txt
        └── Locations
            └── MiSlE0.assetbundle
```

## File formats
### LocationsDescription

The first line is the scenario name, and each lines after that are level names (index is important)

### LocationDialogue

Works the same way as MiSide's localizations.
This relies on `Localization_UIText`'s index and logic (note that you need to reference MiSlE's `Localization_UIText` on MSZ Alpha)

## Setup a custom level project

You'll need to use AssetRipper (or any other game exporter tool) to fetch the original assets and create a custom scene.

You'll likely want to take a look at how the original game scenes work, especially the player controller (and World component on MiSide)

Here's a basic script to export your scene:
```cs
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldExporter
{
    [MenuItem("MiSide/Build Location")]
    static void BuildAllAssetBundles()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();

        string outputPath = "Assets/AssetBundles";

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        AssetImporter assetImporter = AssetImporter.GetAtPath(SceneManager.GetActiveScene().path);

        assetImporter.assetBundleName = SceneManager.GetActiveScene().name + ".assetbundle";
        assetImporter.SaveAndReimport();
        AssetBundleManifest abm = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        AssetDatabase.RemoveUnusedAssetBundleNames();
    }
}
```

Note: This will be included in a custom helper script/SDK soonTM<br>
Note 2: You'll need RealToon v5 to have correct shading on the character on MiSide:Zero, untl either the Menu scene includes it or the SDK includes a replacement