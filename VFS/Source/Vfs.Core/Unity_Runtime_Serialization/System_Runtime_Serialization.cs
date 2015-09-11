#define NET_2_0

#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
    #region DataContractAttribute
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum,
        Inherited = false, AllowMultiple = false)]
    public sealed class DataContractAttribute : Attribute
    {
        string name, ns;

        public DataContractAttribute() {
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        // the default namespace for XmlFormatter (with SharedSchema) is
        // http://schemas.datacontract.org/2004/07/ .
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        // new in 3.5 SP1
        public bool IsReference { get; set; }
    }
    #endregion 

    #region DataMemberAttribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class DataMemberAttribute : Attribute
    {
        bool is_required;
        bool emit_default = true;
        string name;
        int order = -1;

        public DataMemberAttribute() {
        }

        public bool EmitDefaultValue {
            get { return emit_default; }
            set { emit_default = value; }
        }

        public bool IsRequired {
            get { return is_required; }
            set { is_required = value; }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public int Order {
            get { return order; }
            set { order = value; }
        }
    }
    #endregion
}
#endif
