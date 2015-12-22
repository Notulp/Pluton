namespace Pluton.Events
{
    public class RunEventEvent : CountedInstance
    {
        bool stopevent = false;
        GameObjectRef targetPrefab = null;

        public RunEventEvent(TriggeredEventPrefab triggeredEventPrefab)
        {
            targetPrefab = triggeredEventPrefab.targetPrefab;
        }

        public bool Stop {
            get { return stopevent; }
            set { stopevent = value; }
        }

        public GameObjectRef PrefabRef {
            get { return targetPrefab; }
            set { targetPrefab = value; }
        }

        public string Prefab {
            get { return targetPrefab.resourcePath; }
        }
    }
}
