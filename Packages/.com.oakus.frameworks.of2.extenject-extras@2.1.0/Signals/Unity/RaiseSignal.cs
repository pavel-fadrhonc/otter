#if ODIN_INSPECTOR
using Sirenix.Serialization;
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Plugins.Zenject.OptionalExtras.Signals.Unity
{
    public interface ISignal {}

    public class RaiseSignal : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _signalType;
        private object[] _parameters;
        
        [HideInInspector][SerializeField]
        private byte[] _parametersSerializedBA;

        [SerializeField]
        private List<Object> referencedObjects = new List<Object>();

        [SerializeField] 
        private bool _fireAbstract;

        private Type _signalTypeClass;
        private FieldInfo[] _paramFieldInfos;

        [Inject] 
        private SignalBus _signalBus;

        [NonSerialized]
        private bool _init;

        private void OnEnable()
        {
            if (!_init)
                Init();
        }
        
        public void Raise()
        {
            if (_signalTypeClass == null)
                return;
            
            var signal = Activator.CreateInstance(_signalTypeClass);
            
            //Debug.Log($"Raising signal of type {_signalType}");
            //Debug.Log($"params byte array size: {_parametersSerializedBA.Length}");
            //Debug.Log($"object byte array size: {_parameters.Length}");

            for (var index = 0; index < _paramFieldInfos.Length; index++)
            {
                var signalField = _paramFieldInfos[index];
                if (_parameters.Length > index)
                {
                    signalField.SetValue(signal, _parameters[index]);
                }
            }

            if (_fireAbstract)
                _signalBus.AbstractFire(signal);
            else
                _signalBus.Fire(signal);
        }

        public void OnBeforeSerialize()
        {
            if (!_init)
                Init();

            if (!_init)
                return;

    #if ODIN_INSPECTOR
            referencedObjects.Clear();
            _parametersSerializedBA =
     SerializationUtility.SerializeValueWeak(_parameters as object, DataFormat.JSON, out referencedObjects);
    #endif
        }

        public void OnAfterDeserialize()
        {
    #if ODIN_INSPECTOR        
            //Debug.Log($"{nameof(RaiseSignal)} deserializing array of size {_parametersSerializedBA.Length}");
            _parameters =
                SerializationUtility.DeserializeValueWeak(_parametersSerializedBA, DataFormat.JSON, referencedObjects) as object[];
            //Debug.Log($"{nameof(RaiseSignal)} data deserialized into array of size {_parameters.Length}");
    #endif
        }

        public void Init()
        {
            if (_signalType == null)
                return;
            
            _signalTypeClass = Type.GetType(_signalType);
            if (_signalTypeClass == null)
            {
                Debug.LogWarning($"{gameObject.name},{nameof(RaiseSignal)}: Previously saved class of type {_signalType} cannot be found anymore. This can happen in case you renamed it or moved to another namespace/assembly. You need to set select the signal again otherwise the first one in list will get triggered by default.");
                _signalType = null;
                return;
            }

            _paramFieldInfos = _signalTypeClass.GetFields();

            if (_parameters == null)
                _parameters = new object[_paramFieldInfos.Length];
            
            _init = true;
        }
    }
}

