using System.ComponentModel;
using System.Windows.Forms;

namespace NUpdater
{
    public class BindableToolStripMenuItem :
        ToolStripMenuItem, IBindableComponent
    {
        public BindableToolStripMenuItem(string text) :
            base(text)
        {
        }

        #region IBindableComponent Members
        private BindingContext _bindingContext;
        private ControlBindingsCollection _dataBindings;

        [Browsable(false)]
        public BindingContext BindingContext
        {
            get { return _bindingContext ?? (_bindingContext = new BindingContext()); }
            set { _bindingContext = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlBindingsCollection DataBindings => _dataBindings ?? (_dataBindings = new ControlBindingsCollection(this));
        #endregion
    }
}
