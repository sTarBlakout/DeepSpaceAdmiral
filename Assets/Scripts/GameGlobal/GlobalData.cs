﻿using System;
using UnityEngine;

namespace GameGlobal
{
    public class GlobalData : MonoBehaviour
    {
        #region Data

        [SerializeField] private float rtsShipsPosY;
        
        [SerializeField] private float battleshipSlowDownEndPrec = 0.1f;
        [SerializeField] private float battleshipFacingTargetPrec = 0.999f;
        [SerializeField] private float battleshipSideEngineTriggerMove = 0.5f;
        [SerializeField] private float battleshipSideEngineTriggerStay = 0.999f;

        [SerializeField] private Transform particleContainer;
        
        #endregion

        #region Getters
        public float RtsShipsPosY => rtsShipsPosY;
        public float BattleshipSlowDownEndPrec => battleshipSlowDownEndPrec;
        public float BattleshipFacingTargetPrec => battleshipFacingTargetPrec;
        public float BattleshipSideEngineTriggerMove => battleshipSideEngineTriggerMove;
        public float BattleshipSideEngineTriggerStay => battleshipSideEngineTriggerStay;
        public Transform ParticleContainer => particleContainer;

        #endregion
        
        #region Singleton Implementation

        private static GlobalData _instance;
        public static GlobalData Instance
        {
            get 
            {
                if (_instance == null)
                    _instance = FindObjectOfType<GlobalData>();
                return _instance;
            }
        }

        #endregion

        #region Unity Events

        private void Awake() 
        { 
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(particleContainer);
        }
        
        #endregion

        #region Static Functions

        public static bool VectorsApproxEqual(Vector3 v1, Vector3 v2, float precision)
        {
            return Vector3.SqrMagnitude(v1 - v2) < precision;
        }
        
        public static void ActivateParticle(ParticleSystem particleSystem, bool activate)
        {
            if (particleSystem.isPlaying == activate) return;
            
            if (activate)
                particleSystem.Play();
            else
                particleSystem.Stop();    
        }
        
        public static float ClampAngle(float angle, float neededAngle) 
        {
            if (angle > 180)
                angle = Mathf.Clamp(angle, 360f - neededAngle, 360f);
            else
                angle = Mathf.Clamp(angle, 0, neededAngle);

            return angle;
        }
        
        #endregion
    }
}