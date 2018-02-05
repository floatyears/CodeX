using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;

class AnimControllerBuilder
{
    private static Dictionary<string, string> bindpath;
    private static Dictionary<string, string> bindpath1;

    [MenuItem("CTools/角色与特效/创建动画控制器")]
    public static void CreateControllers()
    {
        EditorUtility.DisplayProgressBar("Creating animators...", "Preparing...", 0.0f);

        bindpath = new Dictionary<string, string>() { 
        {"slot_l", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand" }, 
        {"slot_r","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand" },
        {"slot_lf","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0"},
        {"slot_rf","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0"},
        {"slot_b","Bip01/Bip01 Footsteps"},
        {"slot_hit","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1"},
        {"slot_head","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head"},
        {"weapon_base","Bip01"},
        {"weapon_tip","Bip01"}};

        //小鹿的绑定点
        bindpath1 = new Dictionary<string, string>() { 
        {"slot_l","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head/Bone009/Bone012/Bone013/Bone014/Bone015" }, 
        {"slot_r","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head/Bone009/Bone025/Bone026/Bone027/Bone028" },
        {"slot_lf","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand"},
        {"slot_rf","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand"},
        //左后脚：Bip01—Bip01 Pelvis—Bip01 Spine—Bip01 L Thigh—Bip01 L Calf—Bip01 L HorseLink—Bip01 L Foot
        //右后脚：Bip01—Bip01 Pelvis—Bip01 Spine—Bip01 R Thigh—Bip01 R Calf—Bip01 R HorseLink—Bip01 R Foot
        {"slot_b","Bip01/Bip01 Footsteps"},
        {"slot_hit","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head/Bone009"},
        {"slot_head","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head/Bone009/Bone010/Bone011"},
        {"weapon_base","Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 Head/Bone009/Bone012/Bone013/Bone014/Bone015/Bone017"},
        {"weapon_tip","Bip01"}};

        var fbxList = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);


        var p = 0;
        foreach (var fbx in fbxList)
        {
            EditorUtility.DisplayProgressBar("Creating animators...  " + ++p + " of " + fbxList.Length + "", "Creating character [" + fbx.name + "]...", (float)p / fbxList.Length);

            if (PrefabUtility.GetPrefabType(fbx) != PrefabType.ModelPrefab) continue;

            // CreateAnimation((GameObject)fbx, null, p, fbxList.Length);
            CreateAnimController((GameObject)fbx, null, p, fbxList.Length);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
    }

    public static void CreateAnimation(GameObject model, GameObject prefab = null, int p = -1, int len = -1)
    {
        var path = AssetDatabase.GetAssetPath(model);
        path = path.Substring(0, path.LastIndexOf("/") + 1);
        path = path.Substring(path.IndexOf("/"));
        //这个接口只能加载某个母资源下的所有子资源
        //var childAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        

        if (prefab == null) prefab = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (prefab != null)
        {
            var animation = prefab.GetComponent<Animation>();
            if (animation == null)
                animation = prefab.AddComponent<Animation>();

            var files = Directory.GetFiles(Application.dataPath + path);
            // 获取所有动画剪辑
            //List<AnimationClip> animList = new List<AnimationClip>();
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (var asset in files)
            {
                string localpath = asset.Substring(asset.IndexOf("Assets"));
                var anim = AssetDatabase.LoadAssetAtPath<AnimationClip>(localpath);
                if (anim != null)
                {
                    //if (animation.clip == null) animation.clip = anim;
                    clips.Add(anim);
                    Debug.Log("animation: " + anim.frameRate * anim.length + anim.name);
                }
            }

            AnimationUtility.SetAnimationClips(animation, clips.ToArray());
            animation.cullingType = AnimationCullingType.AlwaysAnimate;
            AssetDatabase.DeleteAsset("Assets/hero/Prefabs/" + model.name + ".prefab");

            AddBindPoint(prefab.transform, "", "slot_b");//.transform;

            AddBindPoint(prefab.transform, "", "slot_hit");//.transform;
            //metadata.slot_head = 
            AddBindPoint(prefab.transform, "", "slot_head");//.transform;
            AddBindPoint(prefab.transform, "", "slot_r");//.transform;
            AddBindPoint(prefab.transform, "", "slot_s");//.transform;

            AddBindPoint(prefab.transform, "", "info_" + prefab.name.Substring(0,prefab.name.Length - 4) + "_1");//.transform;

            PrefabUtility.CreatePrefab("Assets/hero/Prefabs/" + model.name + ".prefab", prefab);

            PrefabUtility.DisconnectPrefabInstance(prefab);
            GameObject.DestroyImmediate(prefab);
        }
    }

    public static void CreateAnimController(GameObject model, GameObject prefab = null, int p = -1, int len = -1)
    {
        
        var path        = AssetDatabase.GetAssetPath(model);
        path = path.Substring(0, path.LastIndexOf("/") + 1);
        path = path.Substring(path.IndexOf("/"));
        //这个接口只能加载某个母资源下的所有子资源
        //var childAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        var files = Directory.GetFiles(Application.dataPath + path);
        // 获取所有动画剪辑
        List<AnimationClip> animList = new List<AnimationClip>();
        foreach (var asset in files)
        {
            string localpath = asset.Substring(asset.IndexOf("Assets"));
            var anim = AssetDatabase.LoadAssetAtPath<AnimationClip>(localpath);
            if(anim != null)
            {
                animList.Add(anim);
            }
        }

        // 创建动画控制器
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets" + path.Substring(0, path.LastIndexOf("/")) + "/animator_" + model.name.Replace("character_", "") + ".controller");

        //Log.Info("Editor::AnimControllerBuilder::CreateController", "Create character animation controller [" + AssetDatabase.GetAssetPath(controller) + "].");

        // 添加参数
        var param = new UnityEngine.AnimatorControllerParameter();// AnimatorControllerParameter();
        param.name = "aniState";
        param.type = UnityEngine.AnimatorControllerParameterType.Int;
        param.defaultInt = 0;
        controller.AddParameter(param);

        // param = new UnityEngine.AnimatorControllerParameter();
        // param.name = "knockBack";
        // param.type = UnityEngine.AnimatorControllerParameterType.Bool;
        // param.defaultBool = false;
        // controller.AddParameter(param);

        //param_aniState.defaultInt   = 1;
        //param_knockBack.defaultBool = false;

        // 获取动画层及状态机
        var animLayer           = controller.layers[0];
        var stateMachine        = animLayer.stateMachine;

        stateMachine.anyStatePosition           = new Vector3(-144, -216, 0);
        stateMachine.parentStateMachinePosition = new Vector3(800,    20, 0);

        //var animWait           = GetAnimation(animList, CreatureAnimationNames.IDLE.ToString());
        //var animAttack         = GetAnimation(animList, CreatureAnimationNames.ATTACK_01.ToString(), animWait);
        //UnityEditor.Animations.AnimatorState stateWait = CreateState(stateMachine, CreatureAnimationNames.IDLE.ToString(), GetAnimation(animList, CreatureAnimationNames.IDLE.ToString()), new Vector3(-192, -300, 0));
        //UnityEditor.Animations.AnimatorState stateRun = CreateState(stateMachine, CreatureAnimationNames.RUN.ToString(), GetAnimation(animList, CreatureAnimationNames.RUN.ToString(), animWait), new Vector3(72, -444, 0));
        //UnityEditor.Animations.AnimatorState stateAttack = CreateState(stateMachine, CreatureAnimationNames.ATTACK_01.ToString(), GetAnimation(animList, CreatureAnimationNames.ATTACK_01.ToString(), animWait), new Vector3(468, -300, 0));
        //UnityEditor.Animations.AnimatorState stateAddBuff = CreateState(stateMachine, CreatureAnimationNames.ADD_BUFF.ToString(), GetAnimation(animList, CreatureAnimationNames.ADD_BUFF.ToString(), animWait), new Vector3(-300, -444, 0));
        //UnityEditor.Animations.AnimatorState stateSkill01 = CreateState(stateMachine, CreatureAnimationNames.SKILL_01.ToString(), GetAnimation(animList, CreatureAnimationNames.SKILL_01.ToString(), animAttack), new Vector3(324, -84, 0));
        //UnityEditor.Animations.AnimatorState stateSkill02 = CreateState(stateMachine, CreatureAnimationNames.SKILL_02.ToString(), GetAnimation(animList, CreatureAnimationNames.SKILL_02.ToString(), animAttack), new Vector3(324, -84, 0));
        //UnityEditor.Animations.AnimatorState stateStun = CreateState(stateMachine, CreatureAnimationNames.STUN.ToString(), GetAnimation(animList, CreatureAnimationNames.STUN.ToString(), animWait), new Vector3(0, -108, 0));
        //UnityEditor.Animations.AnimatorState stateKnockBack     = CreateState(stateMachine, CreatureAnimationNames.KNOCKBACK,  GetAnimation(animList, CreatureAnimationNames.KNOCKBACK,   animWait),      new Vector3(-504, -360, 0));
        //UnityEditor.Animations.AnimatorState stateShow          = CreateState(stateMachine, CreatureAnimationNames.SHOW,       GetAnimation(animList, CreatureAnimationNames.SHOW,        animWait),      new Vector3( 384, -228, 0));
        //UnityEditor.Animations.AnimatorState stateWin           = CreateState(stateMachine, CreatureAnimationNames.WIN,        GetAnimation(animList, CreatureAnimationNames.WIN,         animWait),      new Vector3( 480, -480, 0));
        //UnityEditor.Animations.AnimatorState stateDead = CreateState(stateMachine, CreatureAnimationNames.DIE.ToString(), GetAnimation(animList, CreatureAnimationNames.DIE.ToString(), animWait), new Vector3(-456, -312, 0));


        //AddTransition(stateMachine, stateWait, stateHurt, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimations.HURT);                                                 // 待机到受击
        //AddTransition(stateMachine, stateHurt, stateWait, false, UnityEditor.Animations.AnimatorConditionMode.NotEqual, "aniState", CreatureAnimations.KNOCKBACK);                                            // 受击到待机
        //AddTransition(stateMachine, stateHurt, stateKnockBack, false, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimations.KNOCKBACK);                                            // 受击到击退
        //AddTransition(stateMachine, null, stateWait, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.IDLE);                                                 // 任意状态到待机
        //AddTransition(stateMachine, null, stateDead, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.DIE);                                                 // 任意状态到死亡
        //AddTransition(stateMachine, null, stateHurt, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.KNOCKBACK, UnityEditor.Animations.AnimatorConditionMode.IfNot, "knockBack");// 任意状态到受击
        //AddTransition(stateMachine, null, stateRun, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.RUN);                                                  // 任意状态到跑步
        //AddTransition(stateMachine, null, stateAttack, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.ATTACK_01);                                               // 任意状态到攻击
        //AddTransition(stateMachine, null, stateSuperSkill, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.SPELL_SLAY);                                           // 任意状态到必杀
        //AddTransition(stateMachine, null, stateStun, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.STUN);                                                 // 任意状态到眩晕
        //AddTransition(stateMachine, null, stateShow, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.SHOW);                                                 // 任意状态到出场
        //AddTransition(stateMachine, null, stateWin, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", CreatureAnimationNames.WIN);                                                  // 任意状态到胜利

        var empty = CreateState(stateMachine, "Empty", null, new Vector3(0f,0f,0f));
        stateMachine.defaultState = empty;
       // var animWait = GetAnimation(animList, CreatureAnimationNames.WAIT.ToString());
        //var animAttack = GetAnimation(animList, CreatureAnimationNames.ATTACK01.ToString(), animWait);
        for (int i = 0; i < (int)CreatureAnimationNames.COUNT; i++)
        {
            //for (int j = 0; j < (int)CreatureAnimationNames.COUNT; j++)
            //{
                //if (i != j)
                //{
                    var theta = 2 * Mathf.PI * i / (int)CreatureAnimationNames.COUNT;
                    var anim = GetAnimation(animList, ((CreatureAnimationNames)i).ToString(), null);
                    if (anim == null) continue;
                    var state = CreateState(stateMachine, ((CreatureAnimationNames)i).ToString(), anim, new Vector3(1f * Mathf.Cos(theta), 1f * Mathf.Sin(theta), 0));

                    //anim = GetAnimation(animList, ((CreatureAnimationNames)i).ToString(), null);
                    //if (anim == null) continue;
                    //var state1 = CreateState(stateMachine, ((CreatureAnimationNames)i).ToString(), anim, new Vector3(400f + 400f * Mathf.Cos(theta), 400f + 400f * Mathf.Sin(theta), 0));
                    AddTransition(stateMachine, empty, state, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", (CreatureAnimationNames)i);                                                  // 任意状态到胜利
                    AddTransition(stateMachine, state, empty, true, UnityEditor.Animations.AnimatorConditionMode.NotEqual, "aniState", (CreatureAnimationNames)i);                                                  // 任意状态到胜利
                //}
            }
        //    var theta = 2 * Mathf.PI * i / (int)CreatureAnimationNames.COUNT;
        //    var anim = GetAnimation(animList, ((CreatureAnimationNames)i).ToString(), null);
        //    if (anim == null) continue;
        //    var state = CreateState(stateMachine, ((CreatureAnimationNames)i).ToString(), anim, new Vector3(400f * Mathf.Cos(theta), 400f * Mathf.Sin(theta), 0));
        //    AddTransition(stateMachine, null, state, true, UnityEditor.Animations.AnimatorConditionMode.Equals, "aniState", (CreatureAnimationNames)i);                                                  // 任意状态到胜利
        //}

        //AddTransition(stateMachine, stateSuperSkill, stateWait,      false);
        //AddTransition(stateMachine, stateAttack, stateWait, false);
        //AddTransition(stateMachine, stateShow,       stateWait,      false);
        //AddTransition(stateMachine, stateWin,        stateWait,4      false);
            
        for (var i = 0; i < stateMachine.states.Length; ++i)
        {
            var transitions = stateMachine.states[i].state.transitions;//.GetTransitionsFromState(stateMachine.states[i]);
            for (var j = 0; j < transitions.Length; ++j)
            {
                var transition = transitions[j];
                transition.duration = 0.0f;
                transition.offset   = 0.0f;
                    
                //Unity5中exitTime实现有区别
                if(transition.hasExitTime)
                {
                    transition.exitTime = 1f;
                }
                //for (var k = 0; k < transition.conditions.Length; ++k)
                //{
                //    var condition = transition.conditions[k];
                //    //if (condition.mode == TransitionConditionMode.ExitTime)
                //        condition. = 1.0f;
                //}
            }
        }

        if (p >= 0) EditorUtility.DisplayProgressBar("Creating animators...  " + p + " of " + len + "", "Attaching animator to prefab...", (float)p / len);

        //if (prefab == null) prefab = (GameObject)AssetDatabase.LoadAssetAtPath(/*CharacterCreation.CHARACTER_PREFAB_PATH*/"../Prefabes/" + (model.name.StartsWith("character") ? "" : "character_") + model.name + ".prefab", typeof(GameObject));
        if (prefab == null) prefab = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (prefab != null)
        {
            var animator = prefab.GetComponent<Animator>();
            if (animator == null)
                animator = prefab.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;

            //var metadata = prefab.AddComponent<ModelMetaData>();
            //prefab.AddComponent<ModelAnimationEvent>();
            //metadata.slot_b = 
            var tmp = bindpath;
            if (prefab.name.IndexOf("Dryad") >= 0)
            {
                tmp = bindpath1;
            }

            AddBindPoint(prefab.transform, tmp["slot_b"], "slot_b");//.transform;
            //metadata.slot_l = 
            AddBindPoint(prefab.transform, tmp["slot_l"], "slot_l");//.transform;
            //metadata.slot_r = 
            AddBindPoint(prefab.transform, tmp["slot_r"], "slot_r");//.transform;
            //metadata.slot_lf = 
            AddBindPoint(prefab.transform, tmp["slot_lf"], "slot_lf");//.transform;
            //metadata.slot_rf = 
            AddBindPoint(prefab.transform, tmp["slot_rf"], "slot_rf");//.transform;
            //metadata.slot_hit = 
            AddBindPoint(prefab.transform, tmp["slot_hit"], "slot_hit");//.transform;
            //metadata.slot_head = 
            AddBindPoint(prefab.transform, tmp["slot_head"], "slot_head");//.transform;
            //metadata.weapon_base = 
            AddBindPoint(prefab.transform, tmp["weapon_base"], "weapon_base");//.transform;
            //metadata.weapon_tip = 
            AddBindPoint(prefab.transform, tmp["weapon_tip"], "weapon_tip");//.transform;

            PrefabUtility.CreatePrefab("Assets/hero/Prefabs/" + model.name + ".prefab", prefab);

            PrefabUtility.DisconnectPrefabInstance(prefab);
            GameObject.DestroyImmediate(prefab);
            //Log.Info("Editor::AnimControllerBuilder::CreateController", "Attach contoller to prefab [" + AssetDatabase.GetAssetPath(prefab) + "].");
        }
    }
    //[MenuItem("SSS/角色与特效/创建模型绑定点")]
    //public static CreateModelBandPoint()
    //{

    //}

    private static GameObject AddBindPoint(Transform parent, string path,string name)
    {
        GameObject go = new GameObject();
        go.transform.parent = parent;//.FindChild(path);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.name = name; 

        return go;
    }

    private static UnityEditor.Animations.AnimatorState CreateState(UnityEditor.Animations.AnimatorStateMachine machine, string name, AnimationClip anim, Vector3 pos)
    {
        foreach (var item in machine.states)
        {
            if (item.state.name == name)
                return item.state;
        }
        UnityEditor.Animations.AnimatorState state = machine.AddState(name);
        state.motion = anim;
        //state.SetAnimationClip(anim);
        var state1 = ArrayUtility.Find<UnityEditor.Animations.ChildAnimatorState>(machine.states, x=>x.state == state);
        state1.position = pos;
        //state.position = pos;
        return state;
    }

    private static void AddTransition(UnityEditor.Animations.AnimatorStateMachine machine, UnityEditor.Animations.AnimatorState from, UnityEditor.Animations.AnimatorState to, bool removedef, params object[] conditions)
    {
        UnityEditor.Animations.AnimatorStateTransition transition = null;
        transition = from == null ? machine.AddAnyStateTransition(to) : from.AddTransition(to);

        //Unity5中默认是没有condition的
        //if (removedef) transition.RemoveCondition(0);

        transition.offset   = 0.0f;
        transition.duration = 0.0f;
        transition.hasExitTime = false;

        for (var i = 0; i < conditions.Length;)
        {
            if (conditions.Length - i < 2) break;

            //var cond  = transition.AddCondition();
            var mode  = (UnityEditor.Animations.AnimatorConditionMode)conditions[i++];
            var param = conditions[i++].ToString();

            //cond.mode      = mode;
            //cond.parameter = param;

            float threshold = 0f;
            if (i >= conditions.Length) break;
            if (mode != UnityEditor.Animations.AnimatorConditionMode.If && mode != UnityEditor.Animations.AnimatorConditionMode.IfNot)
                threshold = (int)conditions[i++];

            transition.AddCondition(mode, threshold, param);
        }
    }

    private static AnimationClip GetAnimation(List<AnimationClip> list, string aniName, AnimationClip defaultAnim = null)
    {
        var anim = list.Find(item => { var name = item.name.Substring(item.name.IndexOf("_") + 1).ToLower(); return name == aniName.ToLower(); });
        return anim == null ? defaultAnim : anim;
    }
}

internal enum CreatureAnimationNames
{
    WAIT = 0,
    HURT,
    DIE,
    RUN,
    SKILL01,
    SKILL01_START,
    SKILL01_LOOP,
    SKILL01_END,
    SKILL02,
    SKILL02_START,
    SKILL02_LOOP,
    SKILL02_END,
    SKILL03,
    SKILL03_START,
    SKILL03_LOOP,
    SKILL03_END,
    SKILL04,
    SKILL04_START,
    SKILL04_LOOP,
    SKILL04_END,
    STUN,
    RIDING,
    CAPTURE,
    DEFENSE,
    WIN,
	ATTACK01,
    IDLE,
    R_IDLE,
	DISMOUNT,
    IDLE1,
    JUMP,
    JUMP2,
    COUNT,
}