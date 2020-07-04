using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XNode;
using UnityEditor;

namespace AI {
    public enum AIStatus {
        Success,
        Failure,
        Running
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AISerializedVariable : Attribute {
    }
    public abstract class AIBaseNode : Node {
        [HideInInspector]
        [AISerializedVariable]
        [NonSerialized]
        public bool AIInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected override void Init() {
            base.Init();
            AIInitialized = false;
        }
        virtual public void OnInit(){
        }
        abstract public AIStatus OnProcess();
        virtual public void OnInterrupt() {
        }
    }

    [System.Serializable]
    public class AINodeConnection {
        [SerializeField]
        [HideInInspector]
        string name;
    }

    public class AIContext : Dictionary<string, object> { }

    [CreateAssetMenu(menuName = "AI/Graph", order = 0)]
    public class AIGraph : NodeGraph {
        public void OnEnable() {
            Init();
        }
        [NonSerialized]
        public AIContext context;
        [NonSerialized]
        private AIBaseNode startNode = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private void Init() {
            startNode = null;   
        }
        private void Save(AIBaseNode node) {
            FieldInfo[] props = node.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo prop in props) {
                AISerializedVariable v = prop.GetCustomAttribute<AISerializedVariable>();
                if (v == null) {
                    continue;
                }
                context[node.GetInstanceID() + prop.Name] = prop.GetValue(node);
            }
        }
        private void Load(AIBaseNode node) {
            FieldInfo[] props = node.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo prop in props) {
                AISerializedVariable v = prop.GetCustomAttribute<AISerializedVariable>();
                if (v == null) {
                    continue;
                }
                if (context.ContainsKey(node.GetInstanceID() + prop.Name)) {
                    prop.SetValue(node, context[node.GetInstanceID() + prop.Name]);
                } else {
                    Type type = prop.FieldType;
                    var d = type.IsValueType ? Activator.CreateInstance(type) : null;
                    prop.SetValue(node,d);
                }
            }
        }
        public void Tick(AIContext context) {
            if (startNode == null) {
                startNode = nodes.Find(x => x is AIBaseNode && x.Inputs.All(y => !y.IsConnected)) as AIBaseNode;
            }
            this.context = context;
            AIStatus status = Process(startNode);
        }

        public T GetVariable<T>(string name) {
            if (context.ContainsKey(name)) {
                if (context[name] is T) {
                    return (T)context[name];
                }
            }
            return default(T);
        }
        public void SetVariable(string name, object parameter) {
            context[name] = parameter;
        }

        public void Deinitialize(AIBaseNode node) {
            Load(node);
            if (!node.AIInitialized) {
                return;
            }
            node.OnInterrupt();
            node.AIInitialized = false;
            Save(node);
            foreach(NodePort p in node.Outputs) {
                if (!p.IsConnected) {
                    continue;
                }
                AIBaseNode n = (p.Connection.node as AIBaseNode);
                if (!n) {
                    continue;
                }
                Deinitialize(n);
            }
            foreach(NodePort p in node.DynamicOutputs) {
                if (!p.IsConnected) {
                    continue;
                }
                AIBaseNode n = (p.Connection.node as AIBaseNode);
                if (!n) {
                    continue;
                }
                Deinitialize(n);
            }
        }

        public AIStatus Process(AIBaseNode node) {
            Load(node);
            if (node.AIInitialized == false) {
                node.OnInit();
                node.AIInitialized = true;
            }
            AIStatus status = node.OnProcess();
            switch (status) {
                case AIStatus.Running:
                    Save(node);
                    return status;
                case AIStatus.Failure:
                case AIStatus.Success:
                    node.AIInitialized = false;
                    Save(node);
                    return status;
            }
            Save(node);
            return status;
        }  
    }
}