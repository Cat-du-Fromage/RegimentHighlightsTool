using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitStateMachine : MonoBehaviour
    {
        [field: SerializeField] public EStates State { get; private set; }
        public Dictionary<EStates, UnitState> States { get; private set; }
        
        public RegimentStateMachine ParentStateMachine { get; private set; }
        public Unit Unit { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private void Awake()
        {
            Unit = GetComponent<Unit>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitState CurrentState => States[State];
        public bool IsIdle => State == EStates.Idle;
        public bool IsMoving => State == EStates.Move;
        public bool IsFiring => State == EStates.Fire;
        
        public void OnUpdate()
        {
            CurrentState.UpdateState();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void Initialize()
        {
            InitializeStates();
            ParentStateMachine = Unit.RegimentAttach.StateMachine;
        }

        private void InitializeStates()
        {
            States = new Dictionary<EStates, UnitState>()
            {
                {EStates.Idle, new IdleUnitState(this)},
                {EStates.Move, new MoveUnitState(this)},
                {EStates.Fire, new FireUnitState(this)}
            };
            State = EStates.Idle;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Related ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void ToDefaultState() => TryEnterState(UnitState.Default);

        public void RequestChangeState(Order order)
        {
            States[order.StateOrdered].SetupState(order);
            TryEnterState(order.StateOrdered);
        }
        
        public bool TryEnterState(EStates state)
        {
            if (!States[state].ConditionStateEnter()) return false;
            CurrentState.ExitState();
            State = state;
            CurrentState.EnterState();
            return true;
        }
    }
}
