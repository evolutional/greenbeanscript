using System.Collections.ObjectModel;

namespace GreenBeanScript.VirtualMachine
{
    public class ScriptTypeCollection : KeyedCollection<int, ScriptType>
    {
        protected override int GetKeyForItem(ScriptType item)
        {
            return item.TypeCode;
        }
    }
}