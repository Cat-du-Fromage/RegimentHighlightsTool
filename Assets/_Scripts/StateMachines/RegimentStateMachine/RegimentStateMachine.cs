using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KaizerWald
{
    
    public class RegimentStateMachine : MonoBehaviour
    {

        //==================================================
        //TEST
        public RegimentBlackboard Blackboard;
        //==================================================

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field: SerializeField] public Regiment Regiment { get; private set; }
        [field: SerializeField] public EStates State { get; private set; }
        public Dictionary<EStates, RegimentState> States { get; private set; }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Units Reference ◇◇◇◇◇◇                                                                             │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public HashSet<UnitStateMachine> UnitsStateMachine { get; private set; }
        public HashSet<UnitStateMachine> DeadUnitsStateMachine { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            //==================================================
            //TEST
            Blackboard = new RegimentBlackboard();
            //==================================================
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
                //UnitsStateMachine.Add(unit.StateMachine);
                //unit.StateMachine.Initialize();
            }
        }

        private void InitializeStates()
        {
            States = new Dictionary<EStates, RegimentState>()
            {
                {EStates.Idle, new IdleRegimentState(this)},
                {EStates.Move, new MoveRegimentState(this)},
                {EStates.Fire, new FireRegimentState(this)},
                {EStates.Chase, new ChaseRegimentState(this)},
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
                    Blackboard.SetOrderRegimentTarget(((AttackOrder)order).TargetEnemyRegiment);
                    OnFireOrderReceived(order);
                    return;
                default:
                    return;
            }
        }

        private void OnFireOrderReceived(Order attackOrder)
        {
            bool enterFireState = RequestChangeState(attackOrder);

            if (!enterFireState)
            {

            }
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Related ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void ToDefaultState() => TryEnterState(RegimentState.Default);

        public bool RequestChangeState(Order order)
        {
            States[order.StateOrdered].SetupState(order);
            if (!TryEnterState(order.StateOrdered)) return false;
            //Propagate Order to Units
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachine)
            {
                unitStateMachine.RequestChangeState(order);
            }
            return true;
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
