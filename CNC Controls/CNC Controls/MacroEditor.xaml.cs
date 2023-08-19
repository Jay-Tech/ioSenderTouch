/*
 * MacroEditor.xaml.cs - part of CNC Controls library
 *
 * v0.36 / 2021-12-27 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2020-2021, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

· Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

· Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

· Neither the name of the copyright holder nor the names of its contributors may
be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System.Collections.ObjectModel;
using System.Windows;
using CNC.Core;
using System;
using System.Windows.Controls;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for MacroEditor.xaml
    /// </summary>
    public partial class MacroEditor : UserControl
    {
        private CNC.GCode.Macro addMacro = null;
        private  MacroData _macroData;

        public MacroEditor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_macroData == null)
            {
                _macroData = new MacroData
                {
                    Macros = AppConfig.Settings.Base.Macros
                };
                DataContext = _macroData;
            }
      
            if (_macroData.Macros.Count > 0)
                _macroData.Macro = _macroData.Macros[0];
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cbxMacro.Text = string.Empty;
            textBox.Text = string.Empty;
        }

        void btnOk_Click(object sender, RoutedEventArgs e)
        {

            if (_macroData.Macro != null && _macroData.Code == string.Empty)
                _macroData.Macros.Remove(_macroData.Macro);

            if (_macroData.Macro == null && _macroData.LastMacro != null)
            {
                _macroData.LastMacro.Name = cbxMacro.Text;
                _macroData.LastMacro.ConfirmOnExecute = _macroData.ConfirmOnExecute;
            }

            AppConfig.Settings.Base.Macros = _macroData.Macros;
            AppConfig.Settings.Save();
            cbxMacro.Text = string.Empty;
            textBox.Text = string.Empty;
            addMacro = null;

           
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;

            foreach (var macro in _macroData.Macros)
                id = Math.Max(id, macro.Id);

            addMacro = new CNC.GCode.Macro();
            addMacro.Id = id + 1;
            addMacro.Name = cbxMacro.Text;

            _macroData.Macros.Add(addMacro);
            _macroData.Macro = addMacro;
        }

    }

    public class MacroData : ViewModelBase
    {
        private string _text = string.Empty, _name = string.Empty;
        private CNC.GCode.Macro _macro, _lastMacro = null;
        private ObservableCollection<CNC.GCode.Macro> _macros;

        public ObservableCollection<CNC.GCode.Macro> Macros
        {
            get { return _macros; }
            set { _macros = value; OnPropertyChanged(); }
        }

        public CNC.GCode.Macro Macro
        {
            get { return _macro; }
            set {
                _macro = value;
                //              Code = _macro == null ? string.Empty : _macro.Code;
                if (_macro != null)
                {
                    Code = _macro.Code;
                    _lastMacro = _macro;
                }
                CanAdd = _macro == null;
                CanEdit = _macro != null;
                ConfirmOnExecute = _macro == null || _macro.ConfirmOnExecute;
                OnPropertyChanged();
            }
        }

        public CNC.GCode.Macro LastMacro
        {
            get { return _lastMacro; }
        }

        public bool CanAdd
        {
            get { return _macro == null && Macros.Count <= 12; }
            set { OnPropertyChanged(); }
        }

        public bool CanEdit
        {
            get { return _macro != null ; }
            set { OnPropertyChanged(); }
        }

        public bool ConfirmOnExecute
        {
            get { return _macro == null || _macro.ConfirmOnExecute; }
            set { if (_macro != null) _macro.ConfirmOnExecute = value; OnPropertyChanged(); }
        }

        public string Code
        {
            get { return _text; }
            set {
                _text = value == null ? string.Empty : value;
                if (_macro != null)
                    _macro.Code = _text.TrimEnd('\r', '\n');
                OnPropertyChanged();
            }
        }
    }
}
