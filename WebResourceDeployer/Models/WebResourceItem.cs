using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace JasonLattimer.WebResourceDeployer.Models
{
    class WebResourceItem : INotifyPropertyChanged
    {
        private bool _publish;

        public bool Publish
        {
            get { return _publish; }
            set
            {
                if (_publish == value) return;

                _publish = value;
                OnPropertyChanged();
            }
        }
        public Guid WebResourceId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public List<ComboBoxItem> ProjectFiles { get; set; }

        private string _boundFile;

        public string BoundFile
        {
            get { return _boundFile; }
            set
            {
                if (_boundFile == value) return;

                _boundFile = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
