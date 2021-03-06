﻿using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine;
using UnityEngine;
using Lean.Touch;
using GameGlobal;
using RTS.Ships;
using RTS.UI;

namespace RTS.Controls
{
    public class InputManager : MonoBehaviour
    {
        #region Data
        
        [SerializeField] private float doubleTapThreshold;

        public Action OnObjectReselect;
        
        private readonly SelectedObject _selectedObject = new SelectedObject();
        private float _lastTappedTime;
        private byte _doubleTapCounter;
        private Coroutine _doubleTapResetCoroutine;
        private ManagerUI _managerUI;
        private GameEventListener _gameEventListener;
        
        #endregion

        #region Singleton Implementation

        private static InputManager _instance;
        public static InputManager I
        {
            get 
            {
                if (_instance == null) _instance = FindObjectOfType<InputManager>();
                return _instance;
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _managerUI = FindObjectOfType<ManagerUI>();
            _gameEventListener = GetComponent<GameEventListener>();
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        #endregion

        #region Touches Handling
        
        private bool CheckForDoubleTap()
        {
            var isDoubleTap = _lastTappedTime + doubleTapThreshold >= Time.time;
            
            _doubleTapCounter++;
            if (_doubleTapCounter == 2)
                _doubleTapCounter = 0;

            if (_doubleTapResetCoroutine != null)
                StopCoroutine(_doubleTapResetCoroutine);
            _doubleTapResetCoroutine = StartCoroutine(ResetTapStatsInSeconds(doubleTapThreshold));
            
            return isDoubleTap;
        }

        private IEnumerator ResetTapStatsInSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _doubleTapCounter = 0;
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (!Physics.Raycast(finger.GetRay(), out var hitInfo, Mathf.Infinity) || finger.IsOverGui) return;

            var isDoubleTap = CheckForDoubleTap();
            _lastTappedTime = Time.time;
            
            var monoBehaviorObj = hitInfo.collider.GetComponent<MonoBehaviour>();
            if (monoBehaviorObj != null)
            {
                if (_selectedObject.IsInit)
                {
                    // If can interact somehow, don't reselect.
                    if (_selectedObject.TryInteractWithObject(monoBehaviorObj))
                        return;
                }
                
                // Can't interact in any way, reselect.
                if (!_selectedObject.SameObject(monoBehaviorObj))
                {
                    _selectedObject.InitObject(monoBehaviorObj);
                    ShowProperControlsUI();
                    OnObjectReselect?.Invoke();
                }
                else
                {
                    // Zoom on selected
                    if (isDoubleTap)
                    {
                        CameraManager.Instance.SetLeanChaseDestination(monoBehaviorObj.transform);
                    }
                }
            }
            else
            {
                // Touched map, try to move ship there, if selected.
                var moveToPos = new Vector3(hitInfo.point.x, AllData.I.RtsGameData.RtsShipsPosY, hitInfo.point.z);
                _selectedObject.TryMoveToPos(moveToPos);
            }
        }
        
        #endregion

        #region UI Control

        private void ShowProperControlsUI()
        {
            _managerUI.HideAllControlsUI(true);

            if (_selectedObject.Mono is ShipBase)
                _managerUI.ActivatePopup(PopupType.ShipControl, true);
        }

        #endregion

        #region Handle General Game Events
        
        public void HandleGeneralGameEvents()
        {
            switch (_gameEventListener.GameEventName)
            {
                case "CancelSelection": ResetSelection(); break;
                case "StopAllActions": StopAllActions(); break;
            }
        }

        private void StopAllActions()
        {
            _selectedObject.StopAllActions();
        }
        
        private void ResetSelection()
        {
            _selectedObject.UninitObject();
            _managerUI.ActivatePopup(PopupType.ShipControl, false);
        }
        
        #endregion
        
        #region Handle Ship Game Events
        
        public void HandleShipGameEvents()
        {
            switch (_gameEventListener.GameEventName)
            {
                case "Ship_CFM_Panel": OpenChangeFireModePanel(true); break;
                case "Ship_CFM_StopAllGuns": ChangeFireMode(FireMode.NoGuns); break;
                case "Ship_CFM_OnlyOnboardGuns": ChangeFireMode(FireMode.OnlyOnboard); break;
                case "Ship_CFM_OnlyMainGuns": ChangeFireMode(FireMode.OnlyMain); break;
                case "Ship_CFM_AllGuns": ChangeFireMode(FireMode.AllGuns); break;
                
                case "Ship_LS_Panel": OpenLaunchSquadronPanel(true); break;
                case "Ship_LS_Close": OpenLaunchSquadronPanel(false); break;
                case "Ship_LS_Launch": LaunchSquadron(); break;
            }
        }

        private void OpenLaunchSquadronPanel(bool open)
        {
            _managerUI.ActivatePopup(PopupType.ShipControl, !open);
            _managerUI.ActivatePopup(PopupType.LaunchSquadron, open);
        }

        private void LaunchSquadron()
        {
            var launchSqdPopup = _managerUI.CurrOpenPopup.GetComponent<LaunchSquadPopup>();
            if (launchSqdPopup == null) return;
            _selectedObject.LaunchSquadron(AllData.I.RtsGameData.GetSquadron(launchSqdPopup.SelectedBtnId));
        }

        private void OpenChangeFireModePanel(bool open)
        {
            _managerUI.ActivatePopup(PopupType.ShipControl, !open);
            _managerUI.ActivatePopup(PopupType.ChangeFireMode, open);

            var behavior = _selectedObject.GetCurrBehavior(BehaviorType.FireMode);
            if (behavior != null)
                _managerUI.ChangeSelectedButton(PopupType.ChangeFireMode, (int)(FireMode)behavior);
        }

        private void ChangeFireMode(FireMode mode)
        {
            OpenChangeFireModePanel(false);
            _selectedObject.SwitchBehavior(mode);
            _managerUI.ChangeSelectedButton(PopupType.ChangeFireMode, (int)mode);
        }
        
        #endregion

        #region Selected Object API

        public List<int> GetSquadronIds()
        {
            return _selectedObject.GetSquadronIds();
        }

        #endregion
    }
}
