using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class StateMachine : MonoBehaviour
    {
        BaseState currentState;

        private void Start()
        {
            currentState = GetInitialState();
            currentState?.OnEnter();
        }

        private void Update()
        {
            currentState?.OnUpdate();
        }

        private void LateUpdate()
        {
            currentState?.OnFixedUpdate();
        }

        protected virtual BaseState GetInitialState()
        {
            return null;
        }

        public void ChangeState(BaseState newState)
        {
            currentState.OnExit();
            currentState = newState;
            newState.OnEnter();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 200f, 100f));
            string content = currentState != null ? currentState.Name : "(no current state)";
            GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
            GUILayout.EndArea();
        }
    }
}
