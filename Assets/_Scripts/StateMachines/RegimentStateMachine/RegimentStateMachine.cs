using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentStateMachine : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field: SerializeField] public EStates State { get; private set; }
        public Dictionary<EStates, RegimentState> States { get; private set; }
        public Regiment Regiment { get; private set; }
        
        public HashSet<UnitStateMachine> UnitsStateMachine { get; private set; }
        public HashSet<UnitStateMachine> DeadUnitsStateMachine { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private void Awake()
        {
            Regiment = GetComponent<Regiment>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentState CurrentRegimentState => States[State];
        public bool IsIdle => State == EStates.Idle;
        public bool IsMoving => State == EStates.Move;
        public bool IsFiring => State == EStates.Fire;
        
        public void OnUpdate()
        {
            CleanUpNullUnitsStateMachine();
            CurrentRegimentState.UpdateState();
        }
        
        private void CleanUpNullUnitsStateMachine()
        {
            if (DeadUnitsStateMachine.Count == 0) return;
            UnitsStateMachine.ExceptWith(DeadUnitsStateMachine);
            DeadUnitsStateMachine.Clear();
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void Initialize()
        {
            InitializeStates();
            DeadUnitsStateMachine = new HashSet<UnitStateMachine>(Regiment.Units.Count);
            UnitsStateMachine = new HashSet<UnitStateMachine>(Regiment.Units.Count);
            foreach (Unit unit in Regiment.Units)
            {
                UnitsStateMachine.Add(unit.StateMachine);
                unit.StateMachine.Initialize();
            }
        }
        
        private void InitializeStates()
        {
            States = new Dictionary<EStates, RegimentState>()
            {
                {EStates.Idle, new IdleRegimentState(this)},
                {EStates.Move, new MoveRegimentState(this)},
                {EStates.Fire, new FireRegimentState(this)}
            };
            State = EStates.Idle;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Player Orders ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnAbilityTrigger(EStates state)
        {
            States[state].OnAbilityTrigger();
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
            {
                unitStateMachine.States[state].OnAbilityTrigger();
            }
        }
        
        public void OnOrderReceived(Order order)
        {
            State = order.StateOrdered;
            switch (State)
            {
                case EStates.Idle:
                    return;
                case EStates.Move:
                    RequestChangeState(order);
                    return;
                case EStates.Fire:
                    return;
                default:
                    return;
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Related ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void ToDefaultState() => TryEnterState(RegimentState.Default);

        public void RequestChangeState(Order order)
        {
            States[order.StateOrdered].SetupState(order);
            if (!TryEnterState(order.StateOrdered)) return;
            //Propagate Order to Units
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
            {
                unitStateMachine.RequestChangeState(order);
            }
        }
        
        public bool TryEnterState(EStates state)
        {
            if (!States[state].ConditionStateEnter()) return false;
            CurrentRegimentState.ExitState();
            State = state;
            CurrentRegimentState.EnterState();
            return true;
        }
    }
}
