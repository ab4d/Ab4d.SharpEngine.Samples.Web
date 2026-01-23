using Ab4d.SharpEngine.Common;
using Ab4d.SharpEngine.SceneNodes;
using Ab4d.SharpEngine.Utilities;
using System.Numerics;
using Ab4d.SharpEngine.Meshes;

namespace Ab4d.SharpEngine.Samples.Common;

public static class TestScenes
{
    public enum StandardTestScenes
    {
        Teapot = 0,
        TeapotLowResolution,
        HouseWithTrees,
        Dragon
    }

    private static string[] _standardTestScenesFileNames = new string[]
    {
        "teapot-hires.obj",
        "Teapot.obj",
        "house with trees.obj",
        "dragon_vrip_res3.obj"
    };

    public static async Task<GroupNode> GetTestSceneAsync(Scene scene, StandardTestScenes testScene, bool cacheSceneNode = true)
    {
        var testSceneFileName = _standardTestScenesFileNames[(int)testScene];

        GroupNode? readGroupNode = cacheSceneNode ? scene.GetCachedObject<GroupNode>(testSceneFileName) : null;

        if (readGroupNode == null)
        {
            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Models", testSceneFileName);

            var objImporter = new ObjImporter(scene);
            readGroupNode = await objImporter.ImportAsync(fileName);

            if (cacheSceneNode)
                scene.CacheObject(testSceneFileName, readGroupNode);
        }

        return readGroupNode;
    }
    
    public static async Task<GroupNode> GetTestSceneAsync(Scene scene, StandardTestScenes testScene, Vector3 finalSize, bool cacheSceneNode = true)
    {
        return await GetTestSceneAsync(scene, testScene, Vector3.Zero, PositionTypes.Center, finalSize, cacheSceneNode: cacheSceneNode);
    }
    
    public static async Task<GroupNode> GetTestSceneAsync(Scene scene, 
                                                          StandardTestScenes testScene,
                                                          Vector3 position,
                                                          PositionTypes positionType,
                                                          Vector3 finalSize,
                                                          bool preserveAspectRatio = true,
                                                          bool preserveCurrentTransformation = true,
                                                          bool cacheSceneNode = true)
    {
        var readGroupNode = await GetTestSceneAsync(scene, testScene, cacheSceneNode);

        ModelUtils.PositionAndScaleSceneNode(readGroupNode,
                                             position,
                                             positionType,
                                             finalSize,
                                             preserveAspectRatio,
                                             preserveCurrentTransformation);

        return readGroupNode;
    }
    
    public static void GetTestScene(Scene scene, StandardTestScenes testScene, Action<GroupNode> sceneCreatedCallback, bool cacheSceneNode = true)
    {
        var testSceneFileName = _standardTestScenesFileNames[(int)testScene];
        
        GroupNode? cachedGroupNode = cacheSceneNode ? scene.GetCachedObject<GroupNode>(testSceneFileName) : null;

        if (cachedGroupNode != null)
        {
            sceneCreatedCallback(cachedGroupNode);
            return;
        }

        
        string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Models", testSceneFileName);

        var objImporter = new ObjImporter(scene);
        objImporter.Import(fileName, (readGroupNode) =>
        {
            if (cacheSceneNode)
                scene.CacheObject(testSceneFileName, readGroupNode);

            sceneCreatedCallback(readGroupNode);
        });
    }
    
    public static void GetTestScene(Scene scene, StandardTestScenes testScene, Vector3 finalSize, Action<GroupNode> sceneCreatedCallback, bool cacheSceneNode = true)
    {
        GetTestScene(scene, testScene, Vector3.Zero, PositionTypes.Center, finalSize, preserveAspectRatio: true, preserveCurrentTransformation: true, sceneCreatedCallback, cacheSceneNode: cacheSceneNode);
    }

    public static void GetTestScene(Scene scene,
        StandardTestScenes testScene,
        Vector3 position,
        PositionTypes positionType,
        Vector3 finalSize,
        Action<GroupNode> sceneCreatedCallback, 
        bool cacheSceneNode = true)
    {
        GetTestScene(scene, testScene, position, positionType, finalSize, preserveAspectRatio: true, preserveCurrentTransformation: true, sceneCreatedCallback, cacheSceneNode: cacheSceneNode);
    }

    public static void GetTestScene(Scene scene, 
                                    StandardTestScenes testScene,
                                    Vector3 position,
                                    PositionTypes positionType,
                                    Vector3 finalSize,
                                    bool preserveAspectRatio,
                                    bool preserveCurrentTransformation, 
                                    Action<GroupNode> sceneCreatedCallback,
                                    bool cacheSceneNode = true)
    {
        GetTestScene(scene, testScene, (groupNode) =>
        {
            ModelUtils.PositionAndScaleSceneNode(groupNode,
                                                 position,
                                                 positionType,
                                                 finalSize,
                                                 preserveAspectRatio,
                                                 preserveCurrentTransformation);

            sceneCreatedCallback(groupNode);
        }, cacheSceneNode: cacheSceneNode);
    }

    public static GroupNode GetTestScene(StandardTestScenes testScene, bool cacheSceneNode = true)
    {
#if VULKAN
        if (cacheSceneNode && _cachedSceneNodes.TryGetValue(testScene, out var groupNode))
            return groupNode;

        var testSceneFileName = _standardTestScenesFileNames[(int)testScene];

        string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Models", testSceneFileName);

        var objImporter = new ObjImporter();
        var readGroupNode = objImporter.Import(fileName);
        
        if (cacheSceneNode)
                _cachedSceneNodes[testScene] = readGroupNode;
        
        return readGroupNode;
#else
        throw new NotSupportedException("Synchronous GetTestScene is not supported in Web samples. Use GetTestSceneAsync.");
#endif
    }

    public static GroupNode GetTestScene(StandardTestScenes testScene, Vector3 finalSize, bool cacheSceneNode = true)
    {
        return GetTestScene(testScene, Vector3.Zero, PositionTypes.Center, finalSize, cacheSceneNode: cacheSceneNode);
    }
    
    public static GroupNode GetTestScene(StandardTestScenes testScene,
                                         Vector3 position,
                                         PositionTypes positionType,
                                         Vector3 finalSize,
                                         bool preserveAspectRatio = true,
                                         bool preserveCurrentTransformation = true,
                                         bool cacheSceneNode = true)
    {
        var readGroupNode = GetTestScene(testScene, cacheSceneNode: cacheSceneNode);

        ModelUtils.PositionAndScaleSceneNode(readGroupNode,
                                             position,
                                             positionType,
                                             finalSize,
                                             preserveAspectRatio,
                                             preserveCurrentTransformation);

        return readGroupNode;
    }

    public static StandardMesh GetTestMesh(StandardTestScenes testScene, Vector3 finalSize, bool cacheSceneNode = true)
    {
        return GetTestMesh(testScene, Vector3.Zero, PositionTypes.Center, finalSize, cacheSceneNode: cacheSceneNode);
    }


    public static StandardMesh GetTestMesh(StandardTestScenes testScene,
                                           Vector3 position,
                                           PositionTypes positionType,
                                           Vector3 finalSize,
                                           bool preserveAspectRatio = true,
                                           bool preserveCurrentTransformation = true,
                                           bool cacheSceneNode = true)
    {
        var readGroupNode = GetTestScene(testScene, cacheSceneNode: cacheSceneNode);

        if (readGroupNode.Count > 0 && readGroupNode[0] is MeshModelNode singeMeshModelNode)
        {
            if (singeMeshModelNode.Mesh is StandardMesh teapotMesh)
            {
                ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode,
                                                     position,
                                                     positionType,
                                                     finalSize,
                                                     preserveAspectRatio,
                                                     preserveCurrentTransformation);

                Ab4d.SharpEngine.Utilities.ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode, position, positionType, finalSize);
                teapotMesh = Ab4d.SharpEngine.Utilities.MeshUtils.TransformMesh(teapotMesh, singeMeshModelNode.Transform);

                return teapotMesh;
            }
        }

        throw new Exception("Cannot get single mesh from " + testScene.ToString());
    }


    public static void GetTestMesh(Scene scene,
                                   StandardTestScenes testScene,
                                   Vector3 finalSize,
                                   Action<StandardMesh> meshCreatedCallback,
                                   bool cacheSceneNode = true)
    {
        GetTestMesh(scene, testScene, Vector3.Zero, PositionTypes.Center, finalSize, preserveAspectRatio: true, preserveCurrentTransformation: true, meshCreatedCallback, cacheSceneNode: cacheSceneNode);
    }
    
    public static void GetTestMesh(Scene scene,
                                   StandardTestScenes testScene,
                                   Vector3 position,
                                   PositionTypes positionType,
                                   Vector3 finalSize,
                                   Action<StandardMesh> meshCreatedCallback, 
                                   bool cacheSceneNode = true)
    {
        GetTestMesh(scene, testScene, position, positionType, finalSize, preserveAspectRatio: true, preserveCurrentTransformation: true, meshCreatedCallback, cacheSceneNode: cacheSceneNode);
    }

    public static void GetTestMesh(Scene scene,
                                   StandardTestScenes testScene,
                                   Vector3 position,
                                   PositionTypes positionType,
                                   Vector3 finalSize,
                                   bool preserveAspectRatio,
                                   bool preserveCurrentTransformation,
                                   Action<StandardMesh> meshCreatedCallback, 
                                   bool cacheSceneNode = true)
    {
        GetTestScene(scene, testScene, position, positionType, finalSize, preserveAspectRatio, preserveCurrentTransformation, (readGroupNode) =>
        {
            if (readGroupNode.Count > 0 && readGroupNode[0] is MeshModelNode singeMeshModelNode)
            {
                if (singeMeshModelNode.Mesh is StandardMesh standardMesh)
                {
                    ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode,
                                                         position,
                                                         positionType,
                                                         finalSize,
                                                         preserveAspectRatio,
                                                         preserveCurrentTransformation);

                    Ab4d.SharpEngine.Utilities.ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode, position, positionType, finalSize);
                    var mesh = Ab4d.SharpEngine.Utilities.MeshUtils.TransformMesh(standardMesh, singeMeshModelNode.Transform);

                    meshCreatedCallback(mesh);
                    return;
                }
            }

            throw new Exception("Cannot get single mesh from " + testScene.ToString());
        }, cacheSceneNode: cacheSceneNode);
    }
    
    
    public static async Task<StandardMesh> GetTestMeshAsync(Scene scene, StandardTestScenes testScene, Vector3 finalSize, bool cacheSceneNode = true)
    {
        return await GetTestMeshAsync(scene, testScene, Vector3.Zero, PositionTypes.Center, finalSize, cacheSceneNode: cacheSceneNode);
    }


    public static async Task<StandardMesh> GetTestMeshAsync(Scene scene, 
                                                            StandardTestScenes testScene,
                                                            Vector3 position,
                                                            PositionTypes positionType,
                                                            Vector3 finalSize,
                                                            bool preserveAspectRatio = true,
                                                            bool preserveCurrentTransformation = true, 
                                                            bool cacheSceneNode = true)
    {
        var readGroupNode = await GetTestSceneAsync(scene, testScene, cacheSceneNode);

        if (readGroupNode.Count > 0 && readGroupNode[0] is MeshModelNode singeMeshModelNode)
        {
            if (singeMeshModelNode.Mesh is StandardMesh teapotMesh)
            {
                ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode,
                                                     position,
                                                     positionType,
                                                     finalSize,
                                                     preserveAspectRatio,
                                                     preserveCurrentTransformation);

                Ab4d.SharpEngine.Utilities.ModelUtils.PositionAndScaleSceneNode(singeMeshModelNode, position, positionType, finalSize);
                teapotMesh = Ab4d.SharpEngine.Utilities.MeshUtils.TransformMesh(teapotMesh, singeMeshModelNode.Transform);

                return teapotMesh;
            }
        }

        throw new Exception("Cannot get single mesh from " + testScene.ToString());
    }
}