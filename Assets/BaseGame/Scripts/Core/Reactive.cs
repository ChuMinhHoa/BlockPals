using System;
using System.Linq;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

[System.Serializable]
public class Reactive<T> : SerializableReactiveProperty<T>
{
    public Reactive() : base() { }
    public Reactive(T initialValue) : base(initialValue) { }
    public static implicit operator  T (Reactive<T> reactive) => reactive.Value;
    
    
#if UNITY_EDITOR
    [field: OnValueChanged("OnEditorValueChanged")]
    [field: SerializeField] private T EditorValue = default;
    private IDisposable subscription;
    private void OnEditorValueChanged()
    {
        Value = EditorValue;
    }
    public class ReactiveValueDrawer : OdinValueDrawer<Reactive<T>>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ValueEntry.SmartValue.subscription?.Dispose();
            ValueEntry.SmartValue.subscription = ValueEntry.SmartValue.Subscribe(OnValueChanged);
        }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueEntry.Property.Children[1].Draw(label);
        }
        private void OnValueChanged(T value)
        {
            ValueEntry.SmartValue.EditorValue = value;
        }
    }
#endif
}