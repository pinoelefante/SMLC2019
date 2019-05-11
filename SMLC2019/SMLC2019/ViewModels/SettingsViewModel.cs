using SMLC2019.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMLC2019.ViewModels
{
    public class SettingsViewModel : BasicViewModel
    {
        public Configuration Config { get; private set; }
        public SettingsViewModel(Configuration conf, IToast t) : base(t)
        {
            Config = conf;
        }
    }
}
