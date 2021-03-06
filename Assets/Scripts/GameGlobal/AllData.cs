﻿using ScriptableObjects.OverallData;
using UnityEngine;

namespace GameGlobal
{
    public class AllData : MonoBehaviour
    {
        #region Data
        
        [SerializeField] private RtsGameData rtsGameData;
        [SerializeField] private GlobalData globalData;
        [SerializeField] private Transform particleContainer;
        
        #endregion

        #region Getters
        
        public RtsGameData RtsGameData => rtsGameData;
        public GlobalData GlobalData => globalData;
        public Transform ParticleContainer => particleContainer;

        #endregion
        
        #region Singleton Implementation

        private static AllData _instance;
        public static AllData I
        {
            get 
            {
                if (_instance == null) _instance = FindObjectOfType<AllData>();
                return _instance;
            }
        }

        #endregion

        #region Unity Events

        private void Awake()
        {
            Application.targetFrameRate = GlobalData.FPS;
            
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(particleContainer);
        }
        
        #endregion

        #region Static Functions

        #region VFX

        public static void ActivateParticle(ParticleSystem particleSystem, bool activate)
        {
            if (particleSystem.isPlaying == activate) return;
            
            if (activate)
                particleSystem.Play();
            else
                particleSystem.Stop();    
        }

        #endregion

        #region Maths
        
        public static bool VectorsApproxEqual(Vector3 v1, Vector3 v2, float precision)
        {
            return Vector3.SqrMagnitude(v1 - v2) < precision;
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

        #endregion
    }
}