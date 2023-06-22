using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class EventBus
    {
        private static EventBus instance;

        public static EventBus Instance => instance ??= new EventBus();

        private EventBus()
        {
            
        }

        public Action<ISelectableRegiment> OnSinglePreselection;
    }
}
