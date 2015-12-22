namespace Pluton.Events
{
    public class GradeChangeEvent: CountedInstance
    {
        private BuildingGrade.Enum grade;

        public BuildingPart BuildingPart;
        public readonly Player Builder;

        public bool HasPrivilege;
        public bool Rotatable = true;
        public bool PayForUpgrade = true;

        public string DestroyReason = string.Empty;
        public bool DoDestroy = false;

        public GradeChangeEvent(BuildingBlock bb, BuildingGrade.Enum bgrade, BasePlayer player)
        {
            BuildingPart = new BuildingPart(bb);
            Builder = Server.GetPlayer(player);
            grade = bgrade;

            HasPrivilege = (bool)bb.CallMethod("CanChangeToGrade", bgrade, player);
        }

        public BuildingGrade.Enum Grade {
            get { return grade; }
            set { grade = value; }
        }

        public int GradeInt {
            get { return (int)grade; }
            set { grade = (BuildingGrade.Enum)value; }
        }

        public void Destroy(string reason = "Plugin blocks building!")
        {
            DoDestroy = true;
            DestroyReason = reason;
        }
    }
}