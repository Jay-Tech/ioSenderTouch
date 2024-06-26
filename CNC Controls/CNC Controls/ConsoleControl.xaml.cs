﻿/*
 * ConsoleControl.xaml.cs - part of CNC Controls library for Grbl
 *
 * v0.01 / 2020-01-27 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2020, Io Engineering (Terje Io)
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

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CNC.Core;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for ConsoleControl.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        public ConsoleControl()
        {
            InitializeComponent();
            Loaded += ConsoleControl_Loaded;
        }

        private void ConsoleControl_Loaded(object sender, RoutedEventArgs re)
        {
            if (DataContext is GrblViewModel)
            {
                ((INotifyCollectionChanged)ListViewControl.ItemsSource).CollectionChanged += CollectionChange;
               
            }
        }

        private void CollectionChange(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            try
            {
                if (VisualTreeHelper.GetChildrenCount(ListViewControl) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(ListViewControl, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }

               // var item = ListViewControl.Items.Count - 1;
                // ListView.SelectedItem = ListView.Items[item];

                //ListViewControl.SelectedIndex = item;
                //ListViewControl.ScrollIntoView(ListViewControl.SelectedItem);
                
                
                //ListView.ScrollIntoView(ListView.Items[item]);
                // ListView.UpdateLayout();
            }
            catch (System.Exception)
            {

                throw;
            }
        
        }

        private void btn_Clear(object sender, RoutedEventArgs e)
        {
            (DataContext as GrblViewModel)?.ResponseLog.Clear();
        }
    }
}
