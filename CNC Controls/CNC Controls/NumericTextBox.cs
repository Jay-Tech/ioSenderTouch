﻿/*
 * NumericTextBox.cs - part of CNC Controls library
 *
 * v0.42 / 2023-03-01 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2023, Io Engineering (Terje Io)
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

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CNC.Controls.Views;

namespace CNC.Controls
{
    public class NumericTextBox : TextBox
    {
        private NumericProperties np = new NumericProperties();

        private bool updateText = true;
        private VirtualKeyBoard _keyBoard;
        public NumericTextBox()
        {
            Height = 24;
            HorizontalContentAlignment = HorizontalAlignment.Right;
            VerticalContentAlignment = VerticalAlignment.Bottom;
            TextWrapping = TextWrapping.NoWrap;
            Loaded += NumericTextBox_Loaded;
            IsVisibleChanged += NumericTextBox_IsVisibleChanged;
            LostFocus += NumericTextBox_LostFocus;
            MouseDoubleClick += NumericTextBox_MouseDoubleClick;
        }

        private void NumericTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender is NumericTextBox uiElement))
            {
                void Close(object senders, EventArgs es)
                {
                    _keyBoard.VBClosing -= Close;
                    _keyBoard.TextChanged -= TextChanged;
                }

                void TextChanged(object senders, string t)
                {
                    if (double.TryParse(t, out var num))
                        uiElement.Value = num;
                }

                if (_keyBoard.Visibility == Visibility.Visible) return;
                _keyBoard.Show();
                _keyBoard.TextChanged -= TextChanged;
                _keyBoard.TextChanged += TextChanged;
                _keyBoard.VBClosing -= Close;
                _keyBoard.VBClosing += Close;
            }
        }

        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _keyBoard.Close();
        }

        private void NumericTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b)
            {
                if (!b)
                {
                    _keyBoard.Close();
                }
            }
        }

        private void NumericTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (_keyBoard == null)
            {
                _keyBoard = new VirtualKeyBoard
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = 875,
                    Top = 500,
                    Topmost = true
                };
            }
        }


        public new string Text { get { return base.Text; } set { base.Text = value; } }
        public NumberStyles Styles { get { return np.Styles; } }
        public string DisplayFormat { get { return np.DisplayFormat; } }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericTextBox), new PropertyMetadata(double.NaN, new PropertyChangedCallback(OnValueChanged)));
        public double Value
        {
            get { double v = (double)GetValue(ValueProperty); return double.IsNaN(v) ? 0d : v; }
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((NumericTextBox)d).updateText)
                ((NumericTextBox)d).Text = double.IsNaN((double)e.NewValue) || double.IsNegativeInfinity((double)e.NewValue) ? string.Empty : Math.Round((double)e.NewValue, ((NumericTextBox)d).np.Precision).ToString(((NumericTextBox)d).np.DisplayFormat, CultureInfo.InvariantCulture);
        }
        //        public static bool CoerceValueChanged(DependencyObject d, object value)
        //        {
        //            double v = (double)value;
        //            NumericTextBox ntb = (NumericTextBox)d;
        //            return get { return (double.IsNaN(ValueMin) || Value >= ValueMin) && (double.IsNaN(ValueMax) || Value <= ValueMax); }
        //;
        //        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumericTextBox), new PropertyMetadata(NumericProperties.MetricFormat, new PropertyChangedCallback(OnFormatChanged)));
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }
        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericProperties.OnFormatChanged(d, ((NumericTextBox)d).np, (string)e.NewValue);
        }

        public new void Clear()
        {
            updateText = false;
            Value = double.NaN;
            updateText = true;
            base.Text = string.Empty;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                string text = SelectionLength > 0 ? Text.Remove(SelectionStart, SelectionLength) : Text;

                updateText = false;
                Value = double.Parse(text == string.Empty || text == "." ? "0" : (text == "-" || text == "-." ? "-0" : text), np.Styles, CultureInfo.InvariantCulture);
                updateText = true;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)e.OriginalSource;
            string text = textBox.SelectionLength > 0 ? textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength) : textBox.Text;
            text = text.Insert(textBox.CaretIndex, e.Text);
            if (!(e.Handled = !NumericProperties.IsStringNumeric(text, np)))
            {
                updateText = false;
                Value = double.Parse(text == "" || text == "." ? "0" : (text == "-" || text == "-." ? "-0" : text), np.Styles, CultureInfo.InvariantCulture);
                updateText = true;
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            double val = 0d;
            if (double.TryParse(Text == string.Empty ? "NaN" : Text, np.Styles, CultureInfo.InvariantCulture, out val))
            {
                if (!IsReadOnly && IsEnabled)
                {
                    updateText = false;
                    Value = val;
                    updateText = true;
                }

                base.OnTextChanged(e);
            }
            else if(Text == string.Empty || Text == ".")
            {
                updateText = false;
                Value = 0d;
                updateText = true;
            }
            else if (Text == "-" || Text == "-.")
            {
                updateText = false;
                Value = -0d;
                updateText = true;
            }
            else
                Text = Math.Round(Value, (np.Precision)).ToString(np.DisplayFormat, CultureInfo.InvariantCulture);
        }
    }
}
