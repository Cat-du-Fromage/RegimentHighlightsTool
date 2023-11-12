using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{

    
    public class UnitManager : MonoBehaviourSingleton<UnitManager>
    {
        //TODO: GROS PROJETS DE CONVERSION
        //List de TOUTE LES UNITES
        //UNITES PAR REGIMENTS Avec leur TransformAccessArray respectif et leur dictionaire de correspondance
        //NULL OBJECT PATTERN (Crée lors du AWAKE)
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //ANIMATION PART
        [SerializeField] private Dictionary<string, AnimationClip> AnimationClips;
        [SerializeField] private Animator SharedAnimator;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Initialize()
        {
            if (SharedAnimator == null) SharedAnimator = FindFirstObjectByType<Unit>().GetComponent<Animator>();
        }

        private void GetAllCLips()
        {
            RuntimeAnimatorController animatorController = SharedAnimator.runtimeAnimatorController;
            AnimationClip[] clips = animatorController.animationClips;
            
            //AnimationClips = new Dictionary<string, AnimationClip>(clips.Length);
            //clips.ForEach(clip => AnimationClips.Add(clip.name, clip));
            //foreach (KeyValuePair<string, AnimationClip> pair in AnimationClips) { Debug.Log($"animation {pair.Key} : {pair.Value.name}"); }
        }
    }
}
