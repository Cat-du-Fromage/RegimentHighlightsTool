//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.6.1
//     from Assets/_Scripts/RegimentAbility/InputControls/RegimentAbilityControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @RegimentAbilityControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @RegimentAbilityControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""RegimentAbilityControls"",
    ""maps"": [
        {
            ""name"": ""GeneralAbility"",
            ""id"": ""d67d120a-af5f-4939-8c55-d6653fda2fe5"",
            ""actions"": [
                {
                    ""name"": ""MarchRun"",
                    ""type"": ""Button"",
                    ""id"": ""7a38a249-e81a-4db7-9d75-eca1d3d07c0b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""451084a1-7db8-46bb-9c4c-511d765c1ec1"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MarchRun"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // GeneralAbility
        m_GeneralAbility = asset.FindActionMap("GeneralAbility", throwIfNotFound: true);
        m_GeneralAbility_MarchRun = m_GeneralAbility.FindAction("MarchRun", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // GeneralAbility
    private readonly InputActionMap m_GeneralAbility;
    private List<IGeneralAbilityActions> m_GeneralAbilityActionsCallbackInterfaces = new List<IGeneralAbilityActions>();
    private readonly InputAction m_GeneralAbility_MarchRun;
    public struct GeneralAbilityActions
    {
        private @RegimentAbilityControls m_Wrapper;
        public GeneralAbilityActions(@RegimentAbilityControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @MarchRun => m_Wrapper.m_GeneralAbility_MarchRun;
        public InputActionMap Get() { return m_Wrapper.m_GeneralAbility; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GeneralAbilityActions set) { return set.Get(); }
        public void AddCallbacks(IGeneralAbilityActions instance)
        {
            if (instance == null || m_Wrapper.m_GeneralAbilityActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_GeneralAbilityActionsCallbackInterfaces.Add(instance);
            @MarchRun.started += instance.OnMarchRun;
            @MarchRun.performed += instance.OnMarchRun;
            @MarchRun.canceled += instance.OnMarchRun;
        }

        private void UnregisterCallbacks(IGeneralAbilityActions instance)
        {
            @MarchRun.started -= instance.OnMarchRun;
            @MarchRun.performed -= instance.OnMarchRun;
            @MarchRun.canceled -= instance.OnMarchRun;
        }

        public void RemoveCallbacks(IGeneralAbilityActions instance)
        {
            if (m_Wrapper.m_GeneralAbilityActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IGeneralAbilityActions instance)
        {
            foreach (var item in m_Wrapper.m_GeneralAbilityActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_GeneralAbilityActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public GeneralAbilityActions @GeneralAbility => new GeneralAbilityActions(this);
    public interface IGeneralAbilityActions
    {
        void OnMarchRun(InputAction.CallbackContext context);
    }
}
