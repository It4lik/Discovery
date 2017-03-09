namespace discovery
{
    public class Check
    {
        public enum CheckType { custom, tcp };
        private CheckType _type;
        private string _customCheck;

        public Check(CheckType type) {
            _type = type;
        }
        public Check(CheckType type, string command) {
            _type = type;
            _customCheck = command;
        }
    }
}