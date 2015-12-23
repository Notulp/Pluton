namespace Pluton.Events
{
    public class EventTriggeredEvent : CountedInstance
    {
        bool stopevent = false;
        GameObjectRef targetPrefab = null;

        public EventTriggeredEvent(TriggeredEventPrefab triggeredEventPrefab)
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
