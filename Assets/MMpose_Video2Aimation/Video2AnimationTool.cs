using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.Animations;

[Serializable]
public class MMPoseAnimation
{
    public MetaInfo meta_info;
    public InstanceInfo[] instance_info;
}

[Serializable]
public class MetaInfo
{
    public int num_keypoints;
    public Dictionary<int, string> keypoint_id2name;
}

[Serializable]
public class InstanceInfo
{
    public int frame_id;
    public Instance[] instances;
}

[Serializable]
public class Instance
{
    //17 keypoints x 3 axis
    public List<List<float>> keypoints;
}

public class Video2AnimationTool : EditorWindow
{
    [MenuItem("Tool/MMpose Video2Animation")]

    public static void ShowWindow()
    {
        GetWindow<Video2AnimationTool>("MMpose Video2Animation");
    }

    string animationSaveDirectory = "";
    List<List<List<float>>> animationKeypoints = new List<List<List<float>>>();
    List<string> keypointsNames = new List<string>();
    MMPoseAnimation keypointsData = null;

    /*
     * Keypoints name
     * 
        "0": "root",
		"1": "right_hip",
		"2": "right_knee",
		"3": "right_foot",
		"4": "left_hip",
		"5": "left_knee",
		"6": "left_foot",
		"7": "spine",
		"8": "thorax",
		"9": "neck_base",
		"10": "head",
		"11": "left_shoulder",
		"12": "left_elbow",
		"13": "left_wrist",
		"14": "right_shoulder",
		"15": "right_elbow",
		"16": "right_wrist"
     */

    //dictionary defines current keypoint and target keypoint
    public Dictionary<int, int> keypointsTarget = new Dictionary<int, int>
    {
        {0, 7},
        {1, 2},
        {2, 3},
        {4, 5},
        {5, 6},
        {7, 8},
        {8, 9},
        //{9, 10},
        {11, 12},
        {12, 13},
        {14, 15},
        {15, 16}
    };

    //dictionary defines current keypoint and parent keypoint
    public Dictionary<int, int> keypointsParent = new Dictionary<int, int>
    {
        {1, 0},
        {2, 1},
        {3, 2},
        {4, 0},
        {5, 4},
        {6, 5},
        {7, 0},
        {8, 7},
        {9, 8},
        {10, 9},
        {11, 7},
        {12, 11},
        {13, 12},
        {14, 7},
        {15, 14},
        {16, 15}
    };

    public int frameNum;
    public float depthOffset = 20;
    public bool debug = false;
    public GameObject g;
    public AnimationClip clip;

    private void OnGUI()
    {
        //Display model
        if (g == null)
        {
            string prefabPath = "Assets/MMpose_Video2Aimation/Character/character.prefab";
            GameObject character = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            g = Instantiate(character);
        }


        string keypointsDataFile;
        GUILayout.Label("Generate data", EditorStyles.boldLabel);
        //put executable python here 
        if (GUILayout.Button("Open video"))
        {

        }

        GUILayout.Space(10);
        GUILayout.Label("Generate animation", EditorStyles.boldLabel);
        if (GUILayout.Button("Open data"))
        {
            //Read json file and parse to class
            string path = EditorUtility.OpenFilePanel("Open data", "", "json");
            if (path.Length != 0)
            {
                keypointsDataFile = File.ReadAllText(path);
                keypointsData = JsonConvert.DeserializeObject<MMPoseAnimation>(keypointsDataFile);
                animationKeypoints = CreateAnimKeypoints(keypointsData.instance_info);
                keypointsNames = KeypointsNames(keypointsData.meta_info.keypoint_id2name);
            }
        }
        //root rotation z axis offset
        depthOffset = EditorGUILayout.FloatField("Euler offset", depthOffset);

        //animation save directory
        animationSaveDirectory = EditorGUILayout.TextField("Animation save directory", animationSaveDirectory);

        

        //Process data to animation file
        if (GUILayout.Button("Create animation"))
        {
            if (keypointsData == null) EditorUtility.DisplayDialog("No data is loaded", "Load 3D keypoints data before generating animation", "ok");
            //Debug.Log("Create Animation");
            clip = CreateAnim(animationKeypoints, keypointsNames, animationSaveDirectory);
            clip.legacy = true;

            Animation current;
            g.TryGetComponent<Animation>(out current);
            if (current != null) DestroyImmediate(current);

            Animation animaton = g.AddComponent<Animation>();

            animaton.AddClip(clip, clip.name);
            animaton.clip = clip;
            animaton.Play();



        }

        
            /*else
            {
                AnimatorController animatorController = new AnimatorController();
                g.GetComponent<Animator>().runtimeAnimatorController = animatorController;
                animatorController.AddMotion(clip);
            }*/
            



        GUILayout.Space(10);
        GUILayout.Label("Debug", EditorStyles.boldLabel);

        if (debug)
        {
            frameNum = EditorGUILayout.IntField("Frame num", frameNum);

            if (GUILayout.Button("Debug Position"))
            {
                for (int i = 0; i < animationKeypoints[0].Count; i++)
                {
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    debugCubes.Add(g);
                    g.transform.position = new Vector3(animationKeypoints[frameNum][i][0], animationKeypoints[frameNum][i][2], -animationKeypoints[frameNum][i][1]);
                    g.transform.localScale = Vector3.one * 0.05f;
                }
            }
            if (GUILayout.Button("Clear"))
            {
                foreach (GameObject g in debugCubes) DestroyImmediate(g);
                debugCubes.Clear();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Hide"))
            {
                debug = false;
            }
        }
        else
        {
            if (GUILayout.Button("Show"))
            {
                debug = true;
            }
        }
    }

    void OnDestroy()
    {
        if (g != null) DestroyImmediate(g);
    }

    List<GameObject> debugCubes = new List<GameObject>();
    //return frames x 17 keypoints x 3 axis
    List<List<List<float>>> CreateAnimKeypoints(InstanceInfo[] instanceInfo)
    {
        List<List<List<float>>> animationKeypoints = new List<List<List<float>>>();
        for (int i = 0; i < instanceInfo.Length; i++)
        {
            animationKeypoints.Add(instanceInfo[i].instances[0].keypoints);
        }
        return animationKeypoints;
    }

    List<string> KeypointsNames(Dictionary<int, string> id2names)
    {
        List<string> names = new List<string>();
        for(int i = 0; i < id2names.Count; i++)
        {
            names.Add(id2names[i]);
        }
        return names;
    }

    //Get current local euler angles from current and parent euler angles
    Vector3 CalculateLocalEuler(Vector3 child, Vector3 parent)
    {
        Quaternion parentWorldRotation = Quaternion.Euler(parent);
        Quaternion childWorldRotation = Quaternion.Euler(child);

        Quaternion childLocalRotation = Quaternion.Inverse(parentWorldRotation) * childWorldRotation;
        Vector3 childLocalEulerAngles = childLocalRotation.eulerAngles;
        return childLocalEulerAngles;
    }


    //Get current global euler angles from current and next position
    Vector3 CalculateGlobalEuler(Vector3 child, Vector3 target)
    {
        Vector3 targetDir = child - target;
        Quaternion rotation = Quaternion.LookRotation(targetDir, Vector3.forward) * Quaternion.Euler(-90, 0, 0);
        Vector3 globalEulerAngles = rotation.eulerAngles;
        return globalEulerAngles;
    }

    //Get global euler list for all keypoints in a single frame
    List<Vector3> CalculateGlobalEulerList(List<List<float>> keypoints)
    {
        List<Vector3> globalPosList = new List<Vector3>();
        List<Vector3> globalEulerList = new List<Vector3>();

        for (int i = 0; i < keypoints.Count; i++)
        {
            //MMpose has reverse XZ axis
            globalPosList.Add(new Vector3(keypoints[i][0], keypoints[i][2], -keypoints[i][1]));
        }
        
        for (int i = 0; i < keypoints.Count; i++)
        {
            if (keypointsTarget.ContainsKey(i))
            {
                globalEulerList.Add(CalculateGlobalEuler(globalPosList[i], globalPosList[keypointsTarget[i]]));
                //if (i == 0) Debug.Log(globalPosList[i] + "------------" + globalPosList[keypointsTarget[i]] + "----------------" + CalculateGlobalEuler(globalPosList[i], globalPosList[keypointsTarget[i]]));
            }
            else
            {
                globalEulerList.Add(Vector3.zero);
            }
        }
        return globalEulerList;
    }

    List<Vector3> CalculateLocalEulerList(List<Vector3> globalEulerList)
    {
        List<Vector3> localEulerList = new List<Vector3>();
        for (int i = 0; i < globalEulerList.Count; i++)
        {
            if(i == 0)
            {
                localEulerList.Add(new Vector3(globalEulerList[0].x - depthOffset, globalEulerList[0].y, globalEulerList[0].z + 5.0f));
                continue;
            }
            if (keypointsTarget.ContainsKey(i))
            {
                localEulerList.Add(CalculateLocalEuler(globalEulerList[i], globalEulerList[keypointsParent[i]]));
            }
            else
            {
                localEulerList.Add(Vector3.zero);
            }
        }
        return localEulerList;
    }


    void SetAnimCurve(ref AnimationClip clip, List<AnimationCurve> curve, int bodyIdx)
    {
        string relativePath = "";
        switch (bodyIdx)
        {
            case 0:
                relativePath = "mixamorig:Hips";
                break;
            case 1:
                relativePath = "mixamorig:Hips/mixamorig:RightUpLeg";
                break;
            case 2:
                relativePath = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg";
                break;
            case 3:
                relativePath = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot";
                break;
            case 4:
                relativePath = "mixamorig:Hips/mixamorig:LeftUpLeg";
                break;
            case 5:
                relativePath = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg";
                break;
            case 6:
                relativePath = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot";
                break;
            case 7:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1";
                break;
            case 8:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck";
                break;
            case 9:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head";
                break;
            case 10:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End";
                break;
            case 11:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm";
                break;
            case 12:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm";
                break;
            case 13:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand";
                break;
            case 14:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm";
                break;
            case 15:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm";
                break;
            case 16:
                relativePath = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand";
                break;
        }
        
        EditorCurveBinding bindingX = EditorCurveBinding.FloatCurve(relativePath, typeof(Transform), "m_LocalEulerAnglesRaw.x");
        AnimationUtility.SetEditorCurve(clip, bindingX, curve[0]);

        EditorCurveBinding bindingY = EditorCurveBinding.FloatCurve(relativePath, typeof(Transform), "m_LocalEulerAnglesRaw.y");
        AnimationUtility.SetEditorCurve(clip, bindingY, curve[1]);

        EditorCurveBinding bindingZ = EditorCurveBinding.FloatCurve(relativePath, typeof(Transform), "m_LocalEulerAnglesRaw.z");
        AnimationUtility.SetEditorCurve(clip, bindingZ, curve[2]);
        
    }

    void SetAnimationTangentModeLinear(List<AnimationCurve> curve, int frameIdx)
    {
        for (int i = 0; i< curve.Count; i++)
        {
            //AnimationUtility.set
            AnimationUtility.SetKeyLeftTangentMode(curve[i], frameIdx, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(curve[i], frameIdx, AnimationUtility.TangentMode.Auto);
        }
        //Debug.Log(frameIdx);
    }

    public List<List<Vector3>> ApplyGaussianFilterToEulerAngle(List<List<Vector3>> input, int windowSize, float sigma)
    {
        List<List<Vector3>> output = new List<List<Vector3>>();
        for(int i = 0; i < input.Count; i++)
        {
            output.Add(new List<Vector3>());
            for (int j = 0; j < input.Count; j++)
            {
                output[i].Add(Vector3.zero);
            }
        }


        for (int i = 0; i < input[0].Count; i++)
        {
            float[] KeypointAnglesX = new float[input.Count];
            float[] KeypointAnglesY = new float[input.Count];
            float[] KeypointAnglesZ = new float[input.Count];
            for (int j = 0; j < input.Count; j++)
            {
                KeypointAnglesX[j] = input[j][i].x;
                KeypointAnglesY[j] = input[j][i].y;
                KeypointAnglesZ[j] = input[j][i].z;
            }
            KeypointAnglesX = ApplyGaussianFilter(KeypointAnglesX, windowSize, sigma);
            KeypointAnglesY = ApplyGaussianFilter(KeypointAnglesY, windowSize, sigma);
            KeypointAnglesZ = ApplyGaussianFilter(KeypointAnglesZ, windowSize, sigma);
            for (int j = 0; j < input.Count; j++)
            {
                output[j][i] = new Vector3(KeypointAnglesX[j], KeypointAnglesY[j], KeypointAnglesZ[j]);
            }
        }
        return output;
    }

    public float[] ApplyGaussianFilter(float[] input, int windowSize, float sigma)
    {
        int radius = windowSize / 2;
        float[] kernel = CreateGaussianKernel(windowSize, sigma);
        float[] output = new float[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            float sum = 0;
            float weightSum = 0;
            for (int j = -radius; j <= radius; j++)
            {
                int index = i + j;
                if (index >= 0 && index < input.Length)
                {
                    float weight = kernel[j + radius];
                    sum += input[index] * weight;
                    weightSum += weight;
                }
            }
            output[i] = sum / weightSum;
        }

        return output;
    }

    private float[] CreateGaussianKernel(int windowSize, float sigma)
    {
        int radius = windowSize / 2;
        float[] kernel = new float[windowSize];
        float sum = 0;
        for (int i = -radius; i <= radius; i++)
        {
            kernel[i + radius] = (float)(Math.Exp(-(i * i) / (2 * sigma * sigma)) / (Math.Sqrt(2 * Math.PI) * sigma));
            sum += kernel[i + radius];
        }

        // Normalize the kernel
        for (int i = 0; i < kernel.Length; i++)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }


    public AnimationClip CreateAnim(List<List<List<float>>> keypoints, List<string> keypointsNames, string animationSaveDirectory, float frameRate = 30.0f)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        //all keypoints local euler information in each frame
        List<List<Vector3>> globalEulerKeypoints = new List<List<Vector3>>();
        List<List<Vector3>> localEulerKeypoints = new List<List<Vector3>>();
        for (int i = 0; i < keypoints.Count; i++)
        {
            globalEulerKeypoints.Add(CalculateGlobalEulerList(keypoints[i]));
        }
        //globalEulerKeypoints = ApplyGaussianFilterToEulerAngle(globalEulerKeypoints, 5, 1.0f);
        for (int i = 0; i < keypoints.Count; i++)
        {
            localEulerKeypoints.Add(CalculateLocalEulerList(globalEulerKeypoints[i]));
        }

        //Create localEuler animation
        for (int i = 0; i < keypoints[0].Count; i++)
        {
            //keypoints at the end of limb do not need to be animated
            if (!keypointsTarget.ContainsKey(i)) continue;

            //create euler xyz curve for each keypoint
            List<AnimationCurve> animCurve = new List<AnimationCurve>();
            for (int k = 0; k < 3; k++)
            {
                animCurve.Add(new AnimationCurve());
            }

            //add key at each frame
            for (int j = 0; j < keypoints.Count; j++)
            {
                /*if (j != 0)
                {
                    localEulerKeypoints[j][i] = AnimationLocalEulerFix(localEulerKeypoints[j][i], localEulerKeypoints[j - 1][i]);
                }*/
                animCurve[0].AddKey((float)j / frameRate, localEulerKeypoints[j][i].x);
                animCurve[1].AddKey((float)j / frameRate, localEulerKeypoints[j][i].y);

                //Adjustment based on parent transform rotation
                if (i == 11)
                {
                    animCurve[2].AddKey((float)j / frameRate, localEulerKeypoints[j][i].z - 90);
                }
                else if (i == 14)
                {
                    animCurve[2].AddKey((float)j / frameRate, localEulerKeypoints[j][i].z + 90);
                }
                else
                {
                    animCurve[2].AddKey((float)j / frameRate, localEulerKeypoints[j][i].z);
                }
                
                SetAnimationTangentModeLinear(animCurve, j);               
            }
            SetAnimCurve(ref clip, animCurve, i);
            //Debug.Log(AnimationUtility.GetKeyLeftTangentMode(animCurve[0], 10));
        }

        //Create root position animation
        List<AnimationCurve> animCurvePos = new List<AnimationCurve>();
        Vector3 rootOriPos = new Vector3(keypoints[0][0][0], keypoints[0][0][2], -keypoints[0][0][1]);
        for (int k = 0; k < 3; k++)
        {
            animCurvePos.Add(new AnimationCurve());
        }
        for (int i = 0; i < keypoints.Count; i++)
        {
            animCurvePos[0].AddKey((float)i / frameRate, keypoints[i][0][0] - rootOriPos.x);
            animCurvePos[1].AddKey((float)i / frameRate, keypoints[i][0][2] - rootOriPos.y);
            animCurvePos[2].AddKey((float)i / frameRate, -keypoints[i][0][1] - rootOriPos.z);
        }

        EditorCurveBinding bindingX = EditorCurveBinding.FloatCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.x");
        EditorCurveBinding bindingY = EditorCurveBinding.FloatCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.y");
        EditorCurveBinding bindingZ = EditorCurveBinding.FloatCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.z");
        AnimationUtility.SetEditorCurve(clip, bindingX, animCurvePos[0]);
        AnimationUtility.SetEditorCurve(clip, bindingY, animCurvePos[1]);
        AnimationUtility.SetEditorCurve(clip, bindingZ, animCurvePos[2]);

        if (animationSaveDirectory.Equals(""))
        {
            animationSaveDirectory = "MMpose2Animation";
        }

        AssetDatabase.CreateAsset(clip, "Assets/MMpose_Video2Aimation/Animation/"+ animationSaveDirectory + ".anim");
        Debug.Log("Animation saved at : Assets / MMpose_Video2Aimation / Animation / " + animationSaveDirectory + ".anim");
        //AssetDatabase.CreateAsset(clip, "Assets/MMpose_Video2Aimation/Animation/Test.anim");

        return clip;
    }
}
