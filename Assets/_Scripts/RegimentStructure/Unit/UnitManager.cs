using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{

    
    public class UnitManager : MonoBehaviour
    {
        //TODO: GROS PROJETS DE CONVERSION
        //List de TOUTE LES UNITES
        //UNITES PAR REGIMENTS Avec leur TransformAccessArray respectif et leur dictionaire de correspondance
        //NULL OBJECT PATTERN (Crée lors du AWAKE)
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                          ◆◆◆◆◆◆ STATIC PROPERTIES ◆◆◆◆◆◆                                           ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static UnitManager Instance { get; private set; }
        
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
        private void Awake()
        {
            Initialize();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Initialize()
        {
            InitializeSingleton();
            if (SharedAnimator == null) SharedAnimator = FindFirstObjectByType<Unit>().GetComponent<Animator>();
        }
        
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
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
