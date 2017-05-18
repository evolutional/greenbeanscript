namespace GreenBeanScript.VirtualMachine
{
    public class TableNode
    {
        public Variable Item;
        public Variable Key;

        public TableNode(Variable key, Variable item)
        {
            this.Key = key;
            this.Item = item;
        }
    }
}