using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentManager : MonoBehaviourSingleton<RegimentManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public List<Regiment> Regiments { get; private set; } = new ();
        
        //Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, Regiment> RegimentsByID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a player
        public Dictionary<ulong, List<Regiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a team
        public Dictionary<int, List<Regiment>> RegimentsByTeamID { get; private set; } = new ();
        
        private List<Tuple<Regiment, Order>> Orders = new (10);
        
        public event Action<Regiment> OnNewRegiment;
        public event Action<Regiment> OnDeadRegiment;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Start()
        {
            HighlightRegimentManager.Instance.OnPlacementEvent += OnPlayerOrder;
        }

        private void FixedUpdate()
        {
            //OUT OF PLACE!
            Regiments.ForEach(regiment => regiment.OnFixedUpdate());
        }

        private void Update()
        {
            ProcessOrders();
            Regiments.ForEach(regiment => regiment.OnUpdate());
        }

        private void LateUpdate()
        {
            CleanupEmptyRegiments();
            Regiments.ForEach(regiment => regiment.OnLateUpdate());
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public int GetEnemiesTeamNumUnits(int friendlyTeamID)
        {
            int numUnits = 0;
            foreach ((int teamID, List<Regiment> regiments) in RegimentsByTeamID)
            {
                if (teamID == friendlyTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }

        public int GetTeamNumUnits(int searchedTeamID)
        {
            int numUnits = 0;
            foreach ((int teamId, List<Regiment> regiments) in RegimentsByTeamID)
            {
                if (teamId != searchedTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }

        private void CleanupEmptyRegiments()
        {
            if (Regiments.Count == 0) return;
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                Regiment regiment = Regiments[i];
                if (regiment.CurrentFormation.NumUnitsAlive > 0) continue;
                Debug.Log($"Regiment destroyed: {regiment.name}");
                Regiments.Remove(regiment);
                Destroy(regiment);
            }
        }
        
        // Start is called before the first frame update
        private void ProcessOrders()
        {
            foreach ((Regiment regiment, Order order) in Orders)
            {
                regiment.BehaviourTree.OnOrderReceived(order);
            }
            Orders.Clear();
        }
    
        //Remplace Par "Order" Generic paramètre List => le tris des ordre est fait Ici
        private void OnPlayerOrder(GameObject regiment, Order regimentMoveOrder)
        {
            Orders.Add(new Tuple<Regiment, Order>(regiment.GetComponent<Regiment>(), regimentMoveOrder));
        }
        
        public void RegisterRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            RegimentsByID.TryAdd(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
            OnNewRegiment?.Invoke(regiment); //MAYBE USELESS
        }
        
        public void UnRegisterRegiment(Regiment regiment)
        {
            OnDeadRegiment?.Invoke(regiment); //MAYBE USELESS
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
    }
}
